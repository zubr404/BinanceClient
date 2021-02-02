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
		private readonly DateTime? dateStart;
		private readonly DateTime? dateEnd;
		private long? dateStartUnix;
		private long? dateEndUnix;
		private readonly TradeHistoryRepository repository;
		private readonly TradeHistoryBuffer tradeHistoryBuffer;
		private long fromId;

		public event EventHandler<string> LoadStateEvent;
		public TradesHistory(string pair, DateTime? dateStart, DateTime? dateEnd, TradeHistoryRepository repository)
        {
			LoadStateEvent = delegate { };
			this.pair = pair.ToUpper();
			this.dateStart = dateStart;
			this.dateEnd = dateEnd;
			this.repository = repository;
			tradeHistoryBuffer = new TradeHistoryBuffer(repository);
			fromId = GetFromId();
		}

		public bool IsActiveLoad { get; set; }

		public async Task<int> Load()
        {
			if (dateStart.HasValue && dateEnd.HasValue)
            {
				dateStartUnix = dateStart.Value.ToUnixTime();
				dateEndUnix = dateEnd.Value.ToUnixTime();
				return await LoadByDate();
            }
            else
            {
				return await LoadAll();
            }
        }

		/// <summary>
		/// Скачиваем по датам
		/// </summary>
		/// <returns></returns>
		private async Task<int> LoadByDate()
        {
			return await Task.Run(async () =>
			{
				fromId = SearchDateLoad(dateStartUnix.Value);
				if(fromId > 0)
                {
					return await LoadAll();
                }
				return -1;
			});
        }

		/// <summary>
		/// Поиск Id сдлки, соответствующей стартовой даты скачивания
		/// </summary>
		/// <param name="queryDateLocal">Московское время, указанное пользователем</param>
		/// <returns>Id сдлки, соответствующей искомому времени</returns>
		private long SearchDateLoad(long queryDateLocal)
		{
			var firstTime = GetTimeTrade(GetUrlFromFirstTrade(pair), out _);
			if (queryDateLocal < firstTime)
			{
				queryDateLocal = firstTime + 1;
			}

			// ID последней сделки
			GetTimeTrade(GetUrlFromLastTrade(pair), out int lastTradeId);

			var half = lastTradeId / 2;
			var midleIndex = half;
			var countStep = 0;
			while (true)
			{
				countStep++;
				half /= 2;
				var searchTimeUnix = GetTimeTrade(GetUrl(pair, midleIndex), out lastTradeId);

				if (searchTimeUnix == queryDateLocal)
				{
					OnLoadStateEvent($"Стартовая дата найдена   : {queryDateLocal}\t{countStep}");
					return lastTradeId;
				}
				else if (searchTimeUnix < queryDateLocal)
				{
					midleIndex += half;
					if (half == 0)
					{
						for (int i = midleIndex; i <= lastTradeId; i++)
						{
							countStep++;
							searchTimeUnix = GetTimeTrade(GetUrl(pair, i), out lastTradeId);
							if (searchTimeUnix >= queryDateLocal)
							{
								OnLoadStateEvent($"Стартовая дата найдена UP: {queryDateLocal}\t{countStep}");
								return lastTradeId;
							}
						}
					}
				}
				else
				{
					midleIndex -= half;
					if (half == 0)
					{
						countStep++;
						for (int i = midleIndex; i >= 0; i--)
						{
							searchTimeUnix = GetTimeTrade(GetUrl(pair, i), out lastTradeId);
							if (searchTimeUnix <= queryDateLocal)
							{
								OnLoadStateEvent($"Стартовая дата найдена DW: {queryDateLocal}\t{countStep}");
								return lastTradeId;
							}
						}
					}
				}
				if (half == 0)
				{
					OnLoadStateEvent($"Стартовая дата не найдена!!!");
					return 0;
				}
			}
		}

		private static long GetTimeTrade(string url, out int id)
		{
			Thread.Sleep(100);
			var requester = new PublicKeyRequiredRequester();
			var response = requester.Request(url, Resources.PUBLIC_KEY);
			using (Stream stream = response.GetResponseStream())
			{
				StreamReader sr = new StreamReader(stream);
				string s = sr.ReadToEnd();

				List<StockExchenge.MarketTradesHistory.Trade> trades = null;
				try
				{
					trades = JConverter.JsonConver<List<StockExchenge.MarketTradesHistory.Trade>>(s);
				}
				catch (Exception)
				{
					Console.WriteLine($"Response: {s}");
				}

				if (trades?.Count > 0)
				{
					var tradeFirst = trades.First();
					id = tradeFirst.ID;
					return tradeFirst.Time;
				}
				else
				{
					Console.WriteLine($"NOT FOUND");
				}
				id = 0;
				return 0;
			}
		}

		/// <summary>
		/// Скачиваем все последователно
		/// </summary>
		/// <returns></returns>
		private async Task<int> LoadAll()
        {
			return await Task.Run(() =>
			{
				OnLoadStateEvent("TradesHistory: START");
				var url = GetUrl();
				var requester = new PublicKeyRequiredRequester();

				IsActiveLoad = true;
				var countRequest = 0;
                while (IsActiveLoad)
                {
					countRequest++;
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
								if(dateEndUnix.HasValue && trade.Time > dateEndUnix.Value)
                                {
									IsActiveLoad = false;
									break;
                                }
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
							tradeHistoryBuffer.AddRange(tradesDB);
							fromId += trades.Count(); // альтернатива: max Id или last Id

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
										OnLoadStateEvent($"TradesHistory retry-after header: secondSleep = {secondSleep}");
									}
								}
								if (secondSleep == 0) { secondSleep = 60; }
								Thread.Sleep(secondSleep * 1000);
							}
							else if (statusCode == 200)
							{
								if (trades.Count == 0)
                                {
									OnLoadStateEvent($"TradesHistory statusCode: {statusCode}. trades.Count == 0");
									break;
                                }
								if(countRequest % 100 == 0)
                                {
									OnLoadStateEvent($"TradesHistory time load: {trades.Last().Time.UnixToDateTime()}");
								}
							}
                            else
                            {
								OnLoadStateEvent($"TradesHistory: STOP (statusCode: {statusCode})");
								return -1;
                            }
						}
					}
                    catch (Exception ex)
                    {
						// запись лога в БД
						OnLoadStateEvent($"TradesHistory error: возможно указана неверная пара.");
						return -1;
                    }
				}
				OnLoadStateEvent("TradesHistory: STOP");
				return 1;
			});
        }

		private string GetUrl(int limit = 1000)
		{
			return $"{Resources.DOMAIN_V3}historicalTrades?symbol={pair}&fromId={fromId}&limit={limit}";
		}
		private static string GetUrl(string pair, long fromId)
		{
			return $"{Resources.DOMAIN_V3}historicalTrades?symbol={pair}&fromId={fromId}&limit=1";
		}
		private static string GetUrlFromFirstTrade(string pair)
		{
			return $"{Resources.DOMAIN_V3}historicalTrades?symbol={pair}&fromId=0&limit=1";
		}
		private static string GetUrlFromLastTrade(string pair)
		{
			return $"{Resources.DOMAIN_V3}historicalTrades?symbol={pair}&limit=1";
		}

		private long GetFromId()
        {
			long max = 1;
            try
            {
				max = repository.Get(pair).ToList().Max(x => x.TradeId) + 1;
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
