# DLL Sample GreenCube ( IO-117 )
﻿<br>
namespace _53106<br>
<br>
public class _53106<br>
{<br>
　　　public static (string output, string logOutput) ReceiveDataFormat(byte[] data)<br>
　　　{<br>
　　　　　　// Tuple構成用出力変数の定義<br>
　　　　　　string output = string.Empty;<br>
　　　　　　string logOutput = string.Empty;<br>
<br>
　　　　　　// Telemetryの処理<br>
　　　　　　if ((data[6] == 0x76) && (data[7] == 0x1a) || (data[6] == 0x36) && (data[7] == 0x12))<br>
　　　　　　{<br>
　　　　　　　　　// KISS逆変換されたデータ（DLLの戻り値）<br>
　　　　　　　　　output = kissRevConversionString;<br>
　　　　　　}<br>
<br>
　　　　　　// Message Data の処理<br>
　　　　　　if ((data[6] == 0x1d) && (data[7] == 0x03))<br>
　　　　　　{<br>
<br>
　　　　　　　　　// DLLの戻り値<br>
　　　　　　　　　logOutput = outputString;<br>
　　　　　　}<br>
<br>
　　　　　　// Tupleを返す <br>
　　　　　　return (output, logOutput);<br>
　　　}<br>
}<br>
