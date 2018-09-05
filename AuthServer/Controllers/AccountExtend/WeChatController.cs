using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using QRCoder;

namespace IdentityServer.Quickstart
{
    /// <summary>
    /// 微信公众号登录
    /// </summary>
    public class WeChatController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        /// <summary>
        /// 验证微信签名
        /// </summary>
        /// * 将token、timestamp、nonce三个参数进行字典序排序
        /// * 将三个参数字符串拼接成一个字符串进行sha1加密
        /// * 开发者获得加密后的字符串可与signature对比，标识该请求来源于微信。
        /// <returns></returns>
        [HttpGet]
        [Route("api/wechat")]
        public IActionResult WeChatCheck(string signature, string timestamp, string nonce, string echostr, string token)
        {
            string[] ArrTmp = { "wechat", timestamp, nonce };
            //字典排序
            Array.Sort(ArrTmp);
            string tmpStr = string.Join("", ArrTmp);
            //字符加密
            var sha1 = HmacSha1Sign(tmpStr);
            if (sha1.Equals(signature))
            {
                return Content(echostr);
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// HMAC-SHA1加密算法
        /// </summary>
        /// <param name="str">加密字符串</param>
        /// <returns></returns>
        public string HmacSha1Sign(string str)
        {
            var sha1 = System.Security.Cryptography.SHA1.Create();
            var hash = sha1.ComputeHash(Encoding.Default.GetBytes(str));
            string byte2String = null;
            for (int i = 0; i < hash.Length; i++)
            {
                byte2String += hash[i].ToString("x2");
            }
            return byte2String;
        }
        /// <summary>
        /// 生成二维码
        /// </summary>
        /// <returns></returns>
        public IActionResult GetQRcode()
        {
            var takenUrl = "https://open.weixin.qq.com/connect/oauth2/authorize?appid=APPID&redirect_uri=REDIRECT_URI&response_type=code&scope=SCOPE&state=STATE#wechat_redirect";
            takenUrl = takenUrl.Replace("APPID", "wxf7fbdf3eca4b3a30");
            takenUrl = takenUrl.Replace("REDIRECT_URI", "http://space.ngrok.xiaomiqiu.cn/wechatlogin");
            //FIXME ： snsapi_userinfo
            takenUrl = takenUrl.Replace("SCOPE", "snsapi_userinfo");
            // 生成二维码的内容
            QRCodeGenerator qrGenerator = new QRCoder.QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(takenUrl, QRCodeGenerator.ECCLevel.Q);
            QRCode qrcode = new QRCode(qrCodeData);

            //int pixelsPerModule:生成二维码图片的像素大小 ，我这里设置的是5
            // Color darkColor：暗色 一般设置为Color.Black 黑色
            // Color lightColor: 亮色 一般设置为Color.White 白色
            // Bitmap icon: 二维码 水印图标 例如：Bitmap icon = new Bitmap(context.Server.MapPath("~/images/zs.png")); 默认为NULL ，加上这个二维码中间会显示一个图标
            // int iconSizePercent： 水印图标的大小比例 ，可根据自己的喜好设置
            // int iconBorderWidth： 水印图标的边框
            // bool drawQuietZones: 静止区，位于二维码某一边的空白边界,用来阻止读者获取与正在浏览的二维码无关的信息 即是否绘画二维码的空白边框区域 默认为true
            Bitmap qrCodeImage = qrcode.GetGraphic(3, Color.Black, Color.White, null, 15, 6, false);
            MemoryStream ms = new MemoryStream();
            //保存图片
            qrCodeImage.Save(ms, ImageFormat.Jpeg);
            byte[] bytes = ms.GetBuffer();
            return File(bytes, "image/png");

        }
        /// <summary>
        /// 微信登录返回接口
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        [Route("wechatlogin")]
        public IActionResult WeChatLogin(string code)
        {
            return null;
        }
    }
}
