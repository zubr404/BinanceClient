using DataBaseWork.Models;
using DataBaseWork.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Algoritms.BackTest
{
    public class CurrentGridStatistics
    {
        readonly TradeConfigRepository tradeConfigRepository;

        public CurrentGridStatistics(TradeConfigRepository tradeConfigRepository)
        {
            this.tradeConfigRepository = tradeConfigRepository;
        }

        public double DepositUsed { get; set; }
        public int NumberOfOrders { get; set; }
        public double CoverageOfPriceReduction { get; set; }

        public List<OrderData> CalcGrid(List<StopLimitOrderTest> _orders)
        {
            var result = new List<OrderData>();
            var isLong = true;

            List<StopLimitOrderTest> orders;
            if (_orders.Any(x => !x.IsBuyOperation))
            {
                isLong = false;
                orders = _orders.OrderBy(x => x.StopPrice).ToList();
            }
            else
            {
                orders = _orders.OrderByDescending(x => x.StopPrice).ToList();
            }

            var config = tradeConfigRepository.GetLast();
            const int precision = 10;

            if (orders != null)
            {
                double costSum = 0;
                double amountSum = 0;
                foreach (var order in orders)
                {
                    var amount = order.Amount;
                    amountSum += amount;
                    var cost = order.Amount * order.StopPrice;
                    costSum += cost;
                    var avgPrice = costSum / amountSum;

                    var orderData = new OrderData();
                    orderData.Amount = Math.Round(amount, precision);
                    orderData.Equivalent = Math.Round(cost, precision);
                    orderData.PriceInGrid = Math.Round(order.StopPrice, precision);
                    if (order.IsBuyOperation)
                    {
                        orderData.ProfitPrice = Math.Round(avgPrice + avgPrice * config.Profit / 100, precision);
                    }
                    else
                    {
                        isLong = order.IsBuyOperation;
                        orderData.ProfitPrice = Math.Round(avgPrice - avgPrice * config.Profit / 100, precision);
                    }
                    orderData.Rebount = Math.Round(Math.Abs(orderData.PriceInGrid - orderData.ProfitPrice) * 100 / orderData.PriceInGrid, precision);
                    result.Add(orderData);
                }
            }
            if (isLong)
            {
                result = result.OrderByDescending(x => x.PriceInGrid).ToList();
            }
            else
            {
                result = result.OrderBy(x => x.PriceInGrid).ToList();
            }
            CalcStatisticsParametrs(result, isLong);
            return result;
        }

        private void CalcStatisticsParametrs(List<OrderData> orderDatas, bool isLong)
        {
            if(orderDatas != null)
            {
                if(orderDatas.Count > 0)
                {
                    if (isLong)
                    {
                        DepositUsed = orderDatas.Sum(x => x.Equivalent);
                    }
                    else
                    {
                        DepositUsed = orderDatas.Sum(x => x.Amount);
                    }
                    NumberOfOrders = orderDatas.Count;

                    var maxPrice = orderDatas.Max(x => x.PriceInGrid);
                    var minPrice = orderDatas.Min(x => x.PriceInGrid);
                    CoverageOfPriceReduction = ((maxPrice - minPrice) * 100) / maxPrice;
                }
            }
        }
    }
}
