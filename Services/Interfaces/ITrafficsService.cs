using PeShop.Models.Entities;
using PeShop.Models.Enums;
using PeShop.Dtos.Responses;

namespace PeShop.Services.Interfaces;

public interface ITrafficsService
{
    Task<IEnumerable<TrafficStatisticsResponse>> GetTraffics(ETypeTrafic type);
    Task<RequestTraffic> CreateTraffic(RequestTraffic traffic);
}