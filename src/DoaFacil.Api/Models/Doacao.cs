namespace DoaFacil.Api.Models;

public class Doacao
{
    public int Id { get; set; }
    public string NomeDoador { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public CategoriaDoacao Categoria { get; set; }
    public int Quantidade { get; set; }
    public StatusDoacao Status { get; set; } = StatusDoacao.Recebida;
    public DateTime RecebidaEm { get; set; }
    public DateTime? AtualizadaEm { get; set; }

    public int PontoColetaId { get; set; }
    public PontoColeta? PontoColeta { get; set; }
}
