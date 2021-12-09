using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using Runtime.Script;
using TradeApi;
using TradeApi.History;
using TradeApi.Indicators;
using TradeApi.Instruments;
using TradeApi.ToolBelt;


namespace Indicator
{
	/*
	TMA_Bands by Berk Eroglu
	*/
    public class TMABands : IndicatorBuilder 
    {
        public TMABands()
            : base()
        {
			      #region Initialization
            Credentials.ProjectName = "TMABands";
            #endregion 
            
            Lines.Set("Buffer1");
            Lines["Buffer1"].Color = Color.Green;

            Lines.Set("Buffer2");
            Lines["Buffer2"].Color = Color.Green;
            
            Lines.Set("Buffer3");
            Lines["Buffer3"].Color = Color.Green;

            SeparateWindow = false;
        }
        [InputParameter(InputType.Numeric, "ATR Multiplier")]
        [SimpleNumeric(1)]
        public double ATRMultiplier = 3;
        
        [InputParameter(InputType.Numeric, "Half Length")]
        [SimpleNumeric(1D, 99999D)]
        public int HalfLength = 120;
        
        [InputParameter(InputType.Numeric, "ATR Period")]
        [SimpleNumeric(1D, 99999D)]
        public int ATRPeriod = 60;
        
        [InputParameter(InputType.Combobox, "Source")]
        [ComboboxItem("Close", PriceType.Close)]
        [ComboboxItem("Open", PriceType.Open)]
        [ComboboxItem("Typical", PriceType.Typical)]
        [ComboboxItem("Medium", PriceType.Medium)]
        [ComboboxItem("Weighted", PriceType.Weighted)]
        public PriceType SourcePrice = PriceType.Close;
        
        [InputParameter(InputType.Combobox, "High and Low TTS")]
        [ComboboxItem("True", true)]
        [ComboboxItem("False", false)]
        public bool isAlert = false;
        
		private BuiltInIndicator _atr;
		public int BarsCount = 0;
        
		public override void Init()
		{
			_atr = IndicatorsManager.BuildIn.ATR(HistoryDataSeries, ATRPeriod);
		}        
 
        public override void Update(TickStatus args)
        {
        	
			if (HistoryDataSeries.Count < HalfLength)
                return;

			double sum = (HalfLength + 1) * HistoryDataSeries.GetValue(SourcePrice, 0);
            double sumw = (HalfLength + 1);

            for (int j = 1, k = HalfLength; j <= HalfLength; j++,k--)
            {
				int v = 0 + j;
            	sum += k * HistoryDataSeries.GetValue(SourcePrice, v);
                sumw += k;
            }

			double range = _atr.GetValue(0 + 10) * ATRMultiplier;
			
			Lines["Buffer1"].SetValue(sum / sumw);
			Lines["Buffer2"].SetValue(Lines["Buffer1"].GetValue() + range);
			Lines["Buffer3"].SetValue(Lines["Buffer1"].GetValue() - range);
			
			//Alert Condition
			if (isAlert && args == TickStatus.IsBar)
			{	
				if (this.HistoryDataSeries.Count > BarsCount)
				{
					var bid = this.HistoryDataSeries.GetValue(PriceType.Close, 0);
					var path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
				
					if (Lines["Buffer2"].GetValue(0) >= bid)
					{
						Notification.PlaySound(Path.Combine(path,"lowerBand.wav"));
					}
					if (Lines["Buffer3"].GetValue(0) <= bid)
					{
						Notification.PlaySound(Path.Combine(path,"upperBand.wav"));
					}
					BarsCount = this.HistoryDataSeries.Count;
				}
				
			}

        }
        
		public override void Complete()
		{
			
		} 
     }
}
