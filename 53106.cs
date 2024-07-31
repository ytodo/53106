using System;
using System.Linq;
using System.Text;

namespace _53106
{
	public class _53106
	{
		/// <summary>
		/// GreenCube IO-117
		/// 
		/// 1. TELEMETRYデータの不要な3バイトを取り除き、KISS逆変換したものを返す
		/// 2. DIGIPEATERデータのASCIIメッセージを取り出し交信ログ用のデータを返す
		/// 
		///	**********************	
		///	パケットIDの種類
		///	***********************
		///	Header	ID		Len		Type	SatNOGS受入要否
		///	0x8292	0x761a	101		TLM		（必要）
		///	0x8292	0x7619	101		TLM		（不要）
		///	0x8292	0x7618	101		TLM		（不要）
		///	0x8292	0x3612	101		TLM		（必要）
		///	0x8292	0x3611	101		TLM		（実績なし)
		///	0x8292	0x3610	101		TLM		（実績なし)
		///	0x8292	0x765a	 76		TLM		（不要）
		///	0x8292	0x365a	 76		TLM		（不要）
		///	0x8297	0x1d03	 --		Digi
		///	0x8293	0x41d8	 60		?
		///	0x8291	------	  6		?
		///	
		///	**************************************************************
		///	先頭と末尾のデリミタ 0xc0､ 2バイト目のコマンドデータ 0x00 を省くとは
		///	**************************************************************
		///	[c0 00]82 92  2e 00 76 1a 02 b8 65 20 6b 25 00 a5 
		///	08 01 08 19  02 a2 01 a2 00 0b 0c 0b 0c 0d 0d 00 
		///	9f 00 13 20  73 00 14 7f 9e 8b d6 ff 9a 20 8e 01 
		///	f7 00 9f 5a  ac 33 00 11 00 ab ff d0 fb e6 14 31 
		///	27 08 0c 0c  fc fd 00 fd 00 0b 00 00 00 00 00 09 
		///	00 00 00 00  00 00 00 00 00 00 00 00 00 00 00 00 
		///	00 00 00 00  00 83 01 
		///	[c0]
		/// [ ]の3バイトをカットする  101bytes
		/// 
		///	********************
		///	KISS逆変換とは
		///	********************
		///	0xdb 0xdc -----> 0xc0
		///	0xdb 0xdd -----> 0xdb
		///	左辺の2バイトを右辺の1バイトに置き換える。
		///	
		/// </summary>
		/// <param name="data"></param>
		/// <param name="output"></param>
		/// <param name="logOutput"></param>

		public static (string output, string logOutput) ReceiveDataFormat(byte[] data)
		{
			/* Tuple構成用出力変数の定義 */
			string output = string.Empty;
			string logOutput = string.Empty;
			
			/*
			 * Telemetryデータの処理
			 */
			if ((data[6] == 0x76) && (data[7] == 0x1a) || (data[6] == 0x36) && (data[7] == 0x12))
			{
				/* recvDataの先頭と末尾のデリミタ 0xc0 及び 2バイト目のコマンドデータ 0x00 を省く */
				string hexString = string.Join("", data.Skip(2).Take(data.Length - 3).Select(b => b.ToString("X2")));

				// もし文字列に分割文字が含まれていたら、それ以後を破棄する
				string[] splitted = hexString.Split(new string[] { "C0C000", "C0C010" }, StringSplitOptions.None);
				hexString = splitted[0];

				/* KISS逆変換(dbdc => c0 / dbdd => db */
				string kissRevConversionString = hexString.Replace("DBDC", "C0")
														  .Replace("DBDD", "DB")
														  .Replace("dbdc", "c0")
														  .Replace("dbdd", "db");
				/* KISS逆変換されたデータ（DLLの戻り値） */
				output = kissRevConversionString;
			}

			/*
			 * Digipeaterデータの処理
			 */
			if ((data[6] == 0x1d) && (data[7] == 0x03))
			{
				/* recvDataの先頭から8バイトと末尾のデリミタ 0xc0 を省く */
				string recvMsg = Encoding.ASCII.GetString(data);
				//recvMsg = recvMsg.Substring(8, recvMsg.Length - 9);
				recvMsg = recvMsg[8..^1];

				/* 受信メッセージをコールサイン部,GreenCube,メッセージの3部分に分ける(コンマセパレート） */
				string[] parts = recvMsg.Split(',');

				/* コールサイン部(parts[0])を > で区切って from と to に分ける */
				string[] callsigns = parts[0].Split('>');
				string fromCallsign = callsigns[0];
				string toCallsign = callsigns[1];

				/* GreenCube(parts[1])は単に衛星名なので省く */

				/* メッセージ部(ports[2])を一旦スペースで区切ってitemsとして分ける */
				string[] items = parts[2].Trim().Split(' ');        // partsの先頭のスペースを取る

				/* items[0]にはSTORE=?という数字を伴う部分が入っているので = で区切って分ける */
				string[] store = items[0].Split('=');
				string delay = store[1];                            // 数字のみ取り出し

				/* message部分を再構築 */
				int messageLenghth = items.Length - 1;              // STORE=?は処理済みなので長さを１減少する
				string userMessage = string.Empty;
				if (items[1] != null && items[1] != "")             // 最低限STORE=?の次の区分にデータが有れば
				{
					for (int i = 0; i < messageLenghth; i++)        // 次の区分以外にもデータがあれば繰り返す
					{
						userMessage += items[i + 1].Trim() + " ";   // STORE=? 以外の区分をスペース区切りで繋ぐ
					}

					if (parts.Length > 3)
					{
						for (int i = 3; i < parts.Length; i++)
						{
							userMessage += parts[i].Trim();
						}
					}
				}

				/* 各データをまとめてコンマ区切りの"文字列"にする */
				string[] digipeatString = new string[]
				{
					fromCallsign.Trim(),
					toCallsign.Trim(),
					userMessage.Trim(),
					$"Rx:{delay.Trim()}"
				};
				//string outputString = $"[{string.Join(", ", digipeatString.Select(s => $"\"{s}\""))}]";
				string outputString = $"[{string.Join(", ", digipeatString)}]";

				/* DLLの戻り値 */
				logOutput = outputString;
			}
			/* Tupleを返す */ 
			return (output, logOutput);
		}

	}
}
