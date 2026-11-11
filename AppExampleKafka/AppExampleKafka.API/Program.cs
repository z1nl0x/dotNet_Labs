using System.Reflection;
using AppExampleKafka.API.Documentation;
using System.Text;
using AppExampleKafka.API.Middleware;
using AppExampleKafka.API.Security;
using AppExampleKafka.Application.Common.Interfaces;
using System.Text.Json.Serialization;
using AppExampleKafka.Application;
using AppExampleKafka.Infraestructure;
using AppExampleKafka.Infraestructure.Context;
using AppExampleKafka.Infraestructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// --- Camadas da aplicação (Clean Architecture) ---
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUsuarioAtual, UsuarioAtual>();

// --- Autenticação JWT Bearer ---
var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()!;

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddControllers().AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// --- Swagger / OpenAPI ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "AppExampleKafka API",
        Version = "v1",
        Description = """
        API de laboratório para onboarding PF/PJ, contas correntes e transferências internas.

        ### Comece aqui
        1. Em **Auth**, registre um titular usando o exemplo **PF** ou **PJ**, ou faça login.
        2. Copie `accessToken` e cole em **Authorize** (somente o JWT, sem `Bearer`).
        3. Abra uma conta em **Contas** (POST sem corpo) e use o ID retornado nas consultas.
        4. Em **Transacoes**, substitua os GUIDs do exemplo por contas existentes.

        **Saldo inicial zero:** não há endpoint de aporte arbitrário; transferência exige saldo legítimo.
        Refresh token fica em cookie HTTP-only e é rotacionado em `/api/auth/refresh`.
        Após renovar, atualize o JWT em Authorize. Use a mesma origem para preservar cookies.

        Paginação: `pageNumber=1`, `pageSize=20` (máximo 100). Recursos de terceiros não visíveis retornam 404.
        Use apenas dados de teste. Este laboratório não realiza KYC nem é um core bancário de produção.
        """
    });

    // Botão "Authorize": permite enviar o access token JWT nas requisições protegidas.
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Cole apenas o access token JWT (sem o prefixo 'Bearer')."
    });

    // O filtro aplica Bearer somente às operações que realmente exigem autenticação.
    options.OperationFilter<BankingOperationFilter>();

    // Usa os comentários /// dos controllers como descrição dos endpoints.
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath);
});

var app = builder.Build();

// Opt-in apenas para banco novo/descartável. Nunca migrar silenciosamente uma base existente.
if (builder.Configuration.GetValue<bool>("Database:MigrateOnStartup"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "AppExampleKafka API v1");
        options.DocumentTitle = "AppExampleKafka API — Swagger";
        options.DisplayRequestDuration();
        options.EnableFilter();
        options.DisplayOperationId();
        options.DefaultModelsExpandDepth(-1);
        options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
        // Tokens não são persistidos em localStorage; recarregar exige nova autorização.
    });
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
