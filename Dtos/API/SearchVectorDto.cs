namespace PeShop.Dtos.API;
public class SearchVectorDto
{
    public string Id { get; set; } = string.Empty;
    public double SimilarityScore { get; set; }
    public int Rank { get; set; }
}