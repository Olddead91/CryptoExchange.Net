﻿using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for requesting kline/candlestick data
    /// </summary>
    public class GetKlinesOptions : PaginatedEndpointOptions<GetKlinesRequest>
    {
        /// <summary>
        /// The supported kline intervals
        /// </summary>
        public SharedKlineInterval[] SupportIntervals { get; }
        /// <summary>
        /// Max number of data points which can be requested
        /// </summary>
        public int? MaxTotalDataPoints { get; set; }
        /// <summary>
        /// The max age of the data that can be requested
        /// </summary>
        public TimeSpan? MaxAge { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        public GetKlinesOptions(SharedPaginationSupport paginationType, bool timeFilterSupported, int maxLimit, bool needsAuthentication) : base(paginationType, timeFilterSupported, maxLimit, needsAuthentication)
        {
            SupportIntervals = new[]
            {
                SharedKlineInterval.OneMinute,
                SharedKlineInterval.ThreeMinutes,
                SharedKlineInterval.FiveMinutes,
                SharedKlineInterval.FifteenMinutes,
                SharedKlineInterval.ThirtyMinutes,
                SharedKlineInterval.OneHour,
                SharedKlineInterval.TwoHours,
                SharedKlineInterval.FourHours,
                SharedKlineInterval.SixHours,
                SharedKlineInterval.EightHours,
                SharedKlineInterval.TwelveHours,
                SharedKlineInterval.OneDay,
                SharedKlineInterval.OneWeek,
                SharedKlineInterval.OneMonth
            };
        }

        /// <summary>
        /// ctor
        /// </summary>
        public GetKlinesOptions(SharedPaginationSupport paginationType, bool timeFilterSupported, int maxLimit, bool needsAuthentication, params SharedKlineInterval[] intervals) : base(paginationType, timeFilterSupported, maxLimit, needsAuthentication)
        {
            SupportIntervals = intervals;
        }

        /// <summary>
        /// Check whether a specific interval is supported
        /// </summary>
        /// <param name="interval"></param>
        /// <returns></returns>
        public bool IsSupported(SharedKlineInterval interval) => SupportIntervals.Contains(interval);

        /// <inheritdoc />
        public override Error? ValidateRequest(string exchange, GetKlinesRequest request, TradingMode? tradingMode, TradingMode[] supportedApiTypes)
        {
            if (!IsSupported(request.Interval))
                return new ArgumentError("Interval not supported");

            if (MaxAge.HasValue && request.StartTime < DateTime.UtcNow.Add(-MaxAge.Value))
                return new ArgumentError($"Only the most recent {MaxAge} klines are available");

            if (request.Limit > MaxLimit)
                return new ArgumentError($"Only {MaxLimit} klines can be retrieved per request");

            if (MaxTotalDataPoints.HasValue)
            {
                if (request.Limit > MaxTotalDataPoints.Value)
                    return new ArgumentError($"Only the most recent {MaxTotalDataPoints} klines are available");

                if (request.StartTime.HasValue == true)
                {
                    if (((request.EndTime ?? DateTime.UtcNow) - request.StartTime.Value).TotalSeconds / (int)request.Interval > MaxTotalDataPoints.Value)
                        return new ArgumentError($"Only the most recent {MaxTotalDataPoints} klines are available, time filter failed");
                }
            }

            return base.ValidateRequest(exchange, request, tradingMode, supportedApiTypes);
        }

        /// <inheritdoc />
        public override string ToString(string exchange)
        {
            var sb = new StringBuilder(base.ToString(exchange));
            sb.AppendLine($"Supported SharedKlineInterval values: {string.Join(", ", SupportIntervals)}");
            if (MaxAge != null)
                sb.AppendLine($"Max age of data: {MaxAge}");
            if (MaxTotalDataPoints != null)
                sb.AppendLine($"Max total data points available: {MaxTotalDataPoints}");
            return sb.ToString();
        }
    }
}
