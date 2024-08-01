# DLL Sample GreenCube ( IO-117 )

<pre\>
namespace _53106

public class _53106
{
    public static (string output, string logOutput) ReceiveDataFormat(byte[] data)
    {
        // Tuple構成用出力変数の定義
        string output = string.Empty;
        string logOutput = string.Empty;

        // Telemetryの処理
        if ((data[6] == 0x76) && (data[7] == 0x1a) || (data[6] == 0x36) && (data[7] == 0x12))
        {
            // KISS逆変換されたデータ（DLLの戻り値）
            output = kissRevConversionString;
        }

        //---------------------------------------------- デジピータの無い衛星では不要
        // Message Data の処理 
        if ((data[6] == 0x1d) && (data[7] == 0x03))
        {

            // DLLの戻り値
            logOutput = outputString;
        }
        //-------------------------------------------------------------------------
    
        // Tupleを返す
        return (output, logOutput);        // 但し logOutput はそのまま返す
    }
}
</pre>
