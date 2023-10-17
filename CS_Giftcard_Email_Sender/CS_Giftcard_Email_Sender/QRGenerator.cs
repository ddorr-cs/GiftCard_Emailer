using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using QRCoder;

namespace CS_Giftcard_Email_Sender
{
    public class QRGenerator
    {
        private readonly string _url;
        public QRGenerator(string url)
        {
            _url = url;
        }
        public Bitmap GetQRData()
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(_url, QRCodeGenerator.ECCLevel.Q);
            PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);
            byte[] qrCodeBytes = qrCode.GetGraphic(20);
            MemoryStream msQR = new MemoryStream(qrCodeBytes);
            return new Bitmap(msQR);
        }
    }
}
