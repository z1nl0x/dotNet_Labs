using AppExampleKafka.Domain.Entities.Bancario;

namespace AppExampleKafka.Tests;

public class BankingTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(0.001)]
    public void Valor_invalido_e_rejeitado_antes_de_mutacao(decimal valor)
    {
        var origem = new ContaBancaria(Guid.NewGuid());
        var destino = new ContaBancaria(Guid.NewGuid());
        Assert.Throws<ArgumentException>(() => origem.TransferirPara(destino, valor));
        Assert.Equal(0m, origem.Saldo);
        Assert.Equal(0m, destino.Saldo);
    }

    [Theory]
    [InlineData("mesma")]
    [InlineData("inativa")]
    [InlineData("moeda")]
    public void Transferencia_exige_contas_distintas_ativas_mesma_moeda(string cenario)
    {
        var origem = new ContaBancaria(Guid.NewGuid());
        typeof(ContaBancaria).GetProperty("Saldo")!.SetValue(origem, 100m);
        var destino = cenario == "mesma" ? origem : new ContaBancaria(Guid.NewGuid());
        if (cenario == "inativa") typeof(ContaBancaria).GetProperty("Ativa")!.SetValue(destino, false);
        if (cenario == "moeda") typeof(ContaBancaria).GetProperty("Moeda")!.SetValue(destino, "USD");
        Assert.Throws<InvalidOperationException>(() => origem.TransferirPara(destino, 10));
        Assert.Equal(100m, origem.Saldo);
    }

    [Fact]
    public void Limite_do_saldo_do_destino_e_validado_antes_do_debito()
    {
        var origem = new ContaBancaria(Guid.NewGuid());
        var destino = new ContaBancaria(Guid.NewGuid());
        typeof(ContaBancaria).GetProperty("Saldo")!.SetValue(origem, 100m);
        typeof(ContaBancaria).GetProperty("Saldo")!.SetValue(destino, 9999999999999999.99m);
        Assert.Throws<InvalidOperationException>(() => origem.TransferirPara(destino, 1m));
        Assert.Equal(100m, origem.Saldo);
        Assert.Equal(9999999999999999.99m, destino.Saldo);
    }

    [Fact]
    public void Transferencia_sem_saldo_nao_altera_as_contas()
    {
        var origem = new ContaBancaria(Guid.NewGuid());
        var destino = new ContaBancaria(Guid.NewGuid());
        Assert.Throws<InvalidOperationException>(() => origem.TransferirPara(destino, 1m));
        Assert.Equal(0m, origem.Saldo);
        Assert.Equal(0m, destino.Saldo);
    }

    [Fact]
    public void Conta_nova_inicia_sem_saldo_e_ativa()
    {
        var dono = Guid.NewGuid();
        var conta = new ContaBancaria(dono);
        Assert.Equal(dono, conta.UsuarioId);
        Assert.Equal(0m, conta.Saldo);
        Assert.True(conta.Ativa);
        Assert.Equal("BRL", conta.Moeda);
    }
}
