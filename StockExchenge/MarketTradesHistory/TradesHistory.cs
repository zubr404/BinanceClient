using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DataBaseWork;
using DataBaseWork.Models;
using DataBaseWork.Repositories;
using Services;
using StockExchenge.RestApi;

namespace StockExchenge.MarketTradesHistory
{
	/// <summary>
	/// История биржевых сделок
	/// </summary>
	public class TradesHistory
    {
		private readonly string pair;
		private readonly TradeHistoryRepository repository;
		private long fromId;

		public event EventHandler<string> LoadStateEvent;
		public TradesHistory(string pair, TradeHistoryRepository repository)
        {
			LoadStateEvent = delegate { };
			this.pair = pair.ToUpper();
			this.repository = repository;
			this.fromId = GetFromId();
		}

		public async Task<int> Load()
        {
			return await Task.Run(() =>
			{
				OnLoadStateEvent("TradesHistory: START");
				var url = GetUrl();
				var requester = new PublicKeyRequiredRequester();

                while (true)
                {
                    try
                    {
						var response = requester.Request(GetUrl(), Resources.PUBLIC_KEY);
						using (Stream stream = response.GetResponseStream())
						{
							StreamReader sr = new StreamReader(stream);
							string s = sr.ReadToEnd();

							var trades = JConverter.JsonConver<List<StockExchenge.MarketTradesHistory.Trade>>(s);
							var tradesDB = new List<DataBaseWork.Models.TradeHistory>();

							foreach (var trade in trades)
							{
								tradesDB.Add(new DataBaseWork.Models.TradeHistory()
								{
									TradeId = trade.ID,
									Pair = pair,
									Price = trade.Price,
									Qty = trade.Qty,
									Time = trade.Time,
									QuoteQty = trade.QuoteQty,
									IsBestMatch = trade.IsBestMatch,
									IsBuyerMaker = trade.IsBuyerMaker
								});
							}
							repository.AddRange(tradesDB);
							fromId += trades.Count();

							var statusCode = (int)response.StatusCode;
							if (statusCode == 418 || statusCode == 429)
							{
								var secondSleep = 0;
								var headers = response.Headers;
								for (int i = 0; i < headers.Count; i++)
								{
									if (headers.GetKey(i).ToLower() == "retry-after")
									{
										int.TryParse(headers[i], out secondSleep);
									}
								}
								if (secondSleep == 0) { secondSleep = 60; }
								Thread.Sleep(secondSleep * 1000);
							}
							else if (statusCode == 200)
							{
								if (trades.Count == 0)
                                {
									break;
                                }
							}
                            else
                            {
								return -1;
                            }
						}
					}
                    catch (Exception ex)
                    {
						// запись лога в БД
						return -1;
                    }
				}
				return 1;
			});
        }

		private string GetUrl()
		{
			return $"{Resources.DOMAIN_V3}historicalTrades?symbol={pair}&fromId={fromId}&limit=1000";
		}

		private long GetFromId()
        {
			long max = 1;
            try
            {
				max = repository.Get(pair).Max(x => x.TradeId) + 1;
			}
            catch (Exception ex)
            {
				// запись лога в ДБ
				//throw ex;
            }
			return max;
        }

		protected virtual void OnLoadStateEvent(string e)
		{
			LoadStateEvent(this, e);
		}
	}
}
