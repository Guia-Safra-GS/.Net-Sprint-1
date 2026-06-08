using AgroMonitor.Domain.Common;
using AgroMonitor.Domain.Exceptions;

namespace AgroMonitor.Domain.Entities;

/// <summary>
/// Espécie de planta. Guarda a REGRA de negócio do agronômico:
/// a faixa de umidade tolerada, a sensibilidade a geada e o volume de rega.
/// É o lado "1" do relacionamento 1:N com <see cref="Slot"/>.
/// Mapeia a tabela TB_CAD_SPECIES.
/// </summary>
public sealed class Species : BaseEntity
{
    public string CommonName { get; private set; } = string.Empty;

    public string? ScientificName { get; private set; }

    /// <summary>Umidade mínima tolerada (%). Abaixo disso a espécie sofre.</summary>
    public decimal MinHumidity { get; private set; }

    /// <summary>Umidade máxima tolerada (%).</summary>
    public decimal MaxHumidity { get; private set; }

    /// <summary>Temperatura (°C) abaixo da qual há risco de geada. Nulo = não sensível.</summary>
    public decimal? FrostMinTemp { get; private set; }

    /// <summary>Volume de água por rega (ml).</summary>
    public int WateringMl { get; private set; }

    /// <summary>Slots (vagas de plantio) desta espécie. Lado "N" do relacionamento 1:N.</summary>
    public List<Slot> Slots { get; private set; } = [];

    public Species(
        string commonName,
        string? scientificName,
        decimal minHumidity,
        decimal maxHumidity,
        decimal? frostMinTemp,
        int wateringMl)
    {
        SetCommonName(commonName);
        ScientificName = string.IsNullOrWhiteSpace(scientificName) ? null : scientificName.Trim();
        SetHumidityRange(minHumidity, maxHumidity);
        FrostMinTemp = frostMinTemp;
        SetWateringMl(wateringMl);
    }

    // Construtor sem parâmetros exigido pelo EF Core para materializar a entidade.
    private Species()
    {
    }

    /// <summary>Atualiza os dados da espécie, revalidando as regras de domínio.</summary>
    public void Update(
        string commonName,
        string? scientificName,
        decimal minHumidity,
        decimal maxHumidity,
        decimal? frostMinTemp,
        int wateringMl)
    {
        SetCommonName(commonName);
        ScientificName = string.IsNullOrWhiteSpace(scientificName) ? null : scientificName.Trim();
        SetHumidityRange(minHumidity, maxHumidity);
        FrostMinTemp = frostMinTemp;
        SetWateringMl(wateringMl);
    }

    private void SetCommonName(string commonName)
    {
        if (string.IsNullOrWhiteSpace(commonName))
            throw new DomainException("O nome comum da espécie é obrigatório.");

        CommonName = commonName.Trim();
    }

    private void SetHumidityRange(decimal minHumidity, decimal maxHumidity)
    {
        if (minHumidity is < 0 or > 100)
            throw new DomainException("A umidade mínima deve estar entre 0 e 100.");

        if (maxHumidity is < 0 or > 100)
            throw new DomainException("A umidade máxima deve estar entre 0 e 100.");

        if (minHumidity >= maxHumidity)
            throw new DomainException("A umidade mínima deve ser menor que a máxima.");

        MinHumidity = minHumidity;
        MaxHumidity = maxHumidity;
    }

    private void SetWateringMl(int wateringMl)
    {
        if (wateringMl <= 0)
            throw new DomainException("O volume de rega deve ser maior que zero.");

        WateringMl = wateringMl;
    }
}
