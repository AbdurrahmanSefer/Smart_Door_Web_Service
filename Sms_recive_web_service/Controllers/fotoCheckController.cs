using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Sms_recive_web_service.Models;
using System.Web.Http;
using static System.Net.Mime.MediaTypeNames;
using Task = Sms_recive_web_service.Models.Task;

namespace Sms_recive_web_service.Controllers
{  
    public class fotoCheckController : ApiController
    {
        public ApplicationDbContext db = new ApplicationDbContext();
        double C1 = 6.5025, C2 = 58.5225;
        double C3 = 2;
        Task task;
        double[] dizi = new double[15];
        Bitmap originalBitMap;
        [HttpPost]
        [Route("api/fotoCheck/fotoBenzet")]
        public async Task<bool> fotoBenzet()
        {
            try
            {
                var httpRequest = HttpContext.Current.Request;
                var postedFile = httpRequest.Files[0];                          
                var filenameinhosting = postedFile.FileName.Split('\\').LastOrDefault().Split('/').LastOrDefault();
                var fileName = postedFile.FileName;
                var filepath = HttpContext.Current.Server.MapPath("~/door_history/" + filenameinhosting);
                postedFile.SaveAs(filepath);
                originalBitMap = new Bitmap(postedFile.InputStream);
                originalBitMap = ResizeBitmap(originalBitMap, 200, 200);
                originalBitMap = makeGray(originalBitMap);
                try {
                    List<kapi_durumu> veriTaban = db.kapi_durumus.ToList();
                    foreach(var item in veriTaban)
                    {
                        db.kapi_durumus.Remove(item);
                    }
                }
                catch 
                { }
                int sonuc = gett_ssim_sonucu();
                var model = new kapi_durumu
                {
                    Door_case = sonuc,
                    Person_photo = "http://arduino.smsreceive.site/door_history/" + fileName,
                };
                db.kapi_durumus.Add(model);
                db.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }

        }
        [HttpGet]
        [Route("api/fotoCheck/Arduino_request")]
        public async Task<dynamic> Arduino_request()
        {
            try
            {         
                var database= db.kapi_durumus.FirstOrDefault();           
                if(database==null)
                {
                    return 3;
                }
                else
                {
                    int casr_ = database.Door_case;
                    if (database.Door_case==1|| database.Door_case==0)
                    {
                        door_history door_History_ = new door_history
                        {
                            Door_case = database.Door_case,
                            person_photo = database.Person_photo,
                            Time = DateTime.Now.ToString()
                        };
                        db.door_historys.Add(door_History_);
                        db.kapi_durumus.Remove(database);
                        db.SaveChanges();
                    }
                  
                    return casr_;
                }             
            }
            catch(Exception ex)
            {
                return ex.Message;
            }

        }
        [HttpPost]
        [Route("api/fotoCheck/manager_request")]
        public async Task<bool> manager_request(kapi_durumu case_)
        {
            try
            {
                int door_case =Convert.ToInt32(case_.Person_photo);                
                var door = db.kapi_durumus.FirstOrDefault();
                door.Door_case = door_case;            
                db.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }

        }
        [HttpGet]
        [Route("api/fotoCheck/Door_person_photo")]
        public async Task<string> Door_person_photo()
        {
            try
            {
                //2 person in the door is wating
                var door_case = db.kapi_durumus.Where(x=>x.Door_case==2).FirstOrDefault();               
               if(door_case!=null)
                {
                    return door_case.Person_photo;
                }
                else
                {
                    return "Door is Null";
                }            
            }
            catch
            {
                return "eror";
            }

        }

        [HttpGet]
        [Route("api/fotoCheck/Door_history")]
        public async Task<dynamic> Door_history()
        {
            try
            {
                //2 person in the door is wating
                var history = db.door_historys;
                if (history == null)
                {
                    return "eror";
                }
                return history;
            }
            catch
            {
                return "eror";
            }

        }
        public Bitmap ResizeBitmap(Bitmap bmp, int width, int height)
        {
            Bitmap result = new Bitmap(width, height);

            using (Graphics g = Graphics.FromImage(result))
            {
                g.DrawImage(bmp, 0, 0, width, height);
            }
            return result;
        }
        private int gett_ssim_sonucu()
        {
            try
            {

                Double en_buyuk = 0;
                int en_buyuk_i = -1;
                string adres = HttpContext.Current.Server.MapPath("~/EvSahipleri/");
                //adres = adres.Replace("bin\\Debug\\", string.Empty);
                string[] files = Directory.GetFiles(adres);
                task = new Task();
                task.TaskGroups = new List<TaskGroup>();
                foreach (var file in files)
                {
                    TaskGroup g6 = new TaskGroup();
                    g6.GroupName = Path.GetFileName(file);
                    g6.OriginalImageFileName = file;
                    g6.TaskItems = new List<TaskItem>();
                    task.TaskGroups.Add(g6);
                }
                for (int i = 0; i < task.TaskGroups.Count; i++)
                {
                    TaskGroup g2 = task.TaskGroups[i];
                    Bitmap newOrBitmap = new Bitmap(200, 200);
                    Bitmap newPrBitmap = new Bitmap(200, 200);
                    Graphics graphicOrImage = Graphics.FromImage(newOrBitmap);
                    Graphics graphicPrImage = Graphics.FromImage(newPrBitmap);
                    graphicOrImage.DrawImage(originalBitMap, 0, 0, 200, 200);
                    Bitmap originalBitMap2 = new Bitmap(g2.OriginalImageFileName);
                    originalBitMap2 = makeGray(originalBitMap2);
                    graphicPrImage.DrawImage(originalBitMap2, 0, 0, 200, 200);
                    List<Bitmap> orBlockArray = new List<Bitmap>();
                    List<Bitmap> prBlockArray = new List<Bitmap>();
                    for (int k = 0; k < 200 / 8; k++)
                    {
                        for (int j = 0; j < 200 / 8; j++)
                        {
                            Bitmap orBlock = new Bitmap(8, 8);
                            orBlock = Cut(newOrBitmap, k * 8, j * 8, 8, 8);
                            orBlockArray.Add(orBlock);
                            Bitmap prBlock = new Bitmap(8, 8);
                            prBlock = Cut(newPrBitmap, k * 8, j * 8, 8, 8);
                            prBlockArray.Add(prBlock);
                        }
                    }
                    double SSIM = GetSSIM(orBlockArray, prBlockArray);
                    if (SSIM > en_buyuk)
                    {
                        en_buyuk = SSIM;
                        en_buyuk_i = i;
                    }
                    dizi[i] = SSIM;
                }
               

                if (en_buyuk>0.5)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
               
            }
            catch (Exception ex)
            {               
                return 0;
            }
        }
        private Bitmap makeGray(Bitmap bmp)
        {
            try
            {
                for (int i = 0; i < bmp.Height - 1; i++)
                {
                    for (int j = 0; j < bmp.Width; j++)
                    {
                        int deger = (bmp.GetPixel(j, i).R + bmp.GetPixel(j, i).B + bmp.GetPixel(j, i).G) / 3;
                        Color color = Color.FromArgb(deger, deger, deger);
                        bmp.SetPixel(j, i, color);
                    }
                }
                return bmp;
            }
            catch (Exception error)
            {
                //MessageBox.Show(error.Message, "Image Operations");
                return null;
            }

        }
        public void boyutlari_degistir(Bitmap originalBitMap, Bitmap compressedBitMap)
        {
            int width = originalBitMap.Width;
            int height = originalBitMap.Height;

            int modW = width % 8;
            int modH = height % 8;
            int newWidth = width;
            if (modW > 0)
            {
                newWidth = width + (8 - modW);
            }
            int newHeight = height;
            if (modH > 0)
            {
                newHeight = height + (8 - modH);
            }
            Bitmap newOrBitmap = new Bitmap(newWidth, newHeight);
            Bitmap newPrBitmap = new Bitmap(newWidth, newHeight);
            Graphics graphicOrImage = Graphics.FromImage(newOrBitmap);
            Graphics graphicPrImage = Graphics.FromImage(newPrBitmap);
            graphicOrImage.DrawImage(originalBitMap, 0, 0, width, height);
            graphicPrImage.DrawImage(compressedBitMap, 0, 0, width, height);
            List<Bitmap> orBlockArray = new List<Bitmap>();
            List<Bitmap> prBlockArray = new List<Bitmap>();

            for (int i = 0; i < newWidth / 8; i++)
            {
                for (int j = 0; j < newHeight / 8; j++)
                {
                    Bitmap orBlock = new Bitmap(8, 8);
                    orBlock = Cut(newOrBitmap, i * 8, j * 8, 8, 8);
                    orBlockArray.Add(orBlock);

                    Bitmap prBlock = new Bitmap(8, 8);
                    prBlock = Cut(newPrBitmap, i * 8, j * 8, 8, 8);
                    prBlockArray.Add(prBlock);
                }
            }
            //end preparing images          
        }
        private Bitmap Cut(Bitmap b, int StartX, int StartY, int iWidth, int iHeight)
        {
            if (b == null) { return null; }
            int w = b.Width;
            int h = b.Height;
            if (StartX >= w || StartY >= h) { return null; }
            if (StartX + iWidth > w) { iWidth = w - StartX; }
            if (StartY + iHeight > h) { iHeight = h - StartY; }
            try
            {
                Bitmap bmpOut = new Bitmap(iWidth, iHeight, PixelFormat.Format24bppRgb);
                Graphics g = Graphics.FromImage(bmpOut); g.DrawImage(b, new Rectangle(0, 0, iWidth, iHeight), new Rectangle(StartX, StartY, iWidth, iHeight), GraphicsUnit.Pixel);
                g.Dispose();
                return bmpOut;
            }
            catch { return null; }
        }
        private double GetSSIM(List<Bitmap> x, List<Bitmap> y)
        {
            double SSIM = 0;
            for (int i = 0; i < x.Count(); i++)
            {
                double l = GetSSIMLuminance(x[i], y[i]);
                double c = GetSSIMContrast(x[i], y[i]);
                double s = GetSSIMStructural(x[i], y[i]);
                SSIM += (l * c * s);
            }
            SSIM = SSIM / x.Count();

            return SSIM;
        }

        private double GetLuminanceByColor(Color color)
        {
            return 0.2126 * color.R + 0.7152 * color.G + 0.0722 * color.B;
        }
        private double ComputBlockMSE(Bitmap x, Bitmap y)
        {
            double MSE = 0;
            double xLuminance = GetAvgLuminance(x);
            double yLuminance = GetAvgLuminance(y);

            MSE = (xLuminance - yLuminance) * (xLuminance - yLuminance);
            return MSE;
        }
        private double GetSSIMLuminance(Bitmap x, Bitmap y)
        {
            double v = 0;
            double xLuminance = GetAvgLuminance(x);
            double yLuminance = GetAvgLuminance(y);
            v = (2 * xLuminance * yLuminance + C1) / (xLuminance * xLuminance + yLuminance * yLuminance + C1);
            return v;
        }

        private double GetSSIMContrast(Bitmap x, Bitmap y)
        {
            double v = 0;
            double xVariance = GetVariance(x);
            double yVariance = GetVariance(y);
            v = (2 * xVariance * yVariance + C2) / (xVariance * xVariance + yVariance * yVariance + C2);
            return v;
        }

        private double GetSSIMStructural(Bitmap x, Bitmap y)
        {
            C3 = C2 / 2;
            double v = 0;
            double co = GetCovariance(x, y);
            double vx = GetVariance(x);
            double vy = GetVariance(y);
            v = (co + C3) / (vx * vy + C3);
            return v;
        }
        //Get average luminance value of image
        private double GetAvgLuminance(Bitmap bitmap)
        {
            double avgLuminance = 0;
            for (int i = 0; i < bitmap.Width; i++)
            {
                for (int j = 0; j < bitmap.Height; j++)
                {
                    Color color = bitmap.GetPixel(i, j);
                    avgLuminance += (0.2126 * color.R + 0.7152 * color.G + 0.0722 * color.B);
                }
            }
            return avgLuminance / (bitmap.Width * bitmap.Height);
        }
        //Get variance value of image, before sqrt
        private double GetVariance(Bitmap bitmap)
        {

            double v = 0;
            for (int i = 0; i < bitmap.Width; i++)
            {
                for (int j = 0; j < bitmap.Height; j++)
                {
                    try
                    {
                        Color or_color = bitmap.GetPixel(i, j);
                        double or_luminance = 0.2126 * or_color.R + 0.7152 * or_color.G + 0.0722 * or_color.B;
                        v += or_luminance;
                    }
                    finally
                    {
                    }
                }
            }
            double avgV = v / 64;
            v = 0;

            for (int i = 0; i < bitmap.Width; i++)
            {
                for (int j = 0; j < bitmap.Height; j++)
                {
                    try
                    {
                        Color or_color = bitmap.GetPixel(i, j);
                        double or_luminance = 0.2126 * or_color.R + 0.7152 * or_color.G + 0.0722 * or_color.B;
                        v += (or_luminance - avgV) * (or_luminance - avgV);
                    }
                    finally
                    {
                    }
                }
            }

            return Math.Sqrt(v / (bitmap.Width * bitmap.Height));
        }
        //Get covariance value of image1 and image2
        private double GetCovariance(Bitmap x, Bitmap y)
        {
            double v = 0;
            for (int i = 0; i < x.Width; i++)
            {
                for (int j = 0; j < x.Height; j++)
                {
                    try
                    {
                        Color or_color = x.GetPixel(i, j);
                        double or_luminance = 0.2126 * or_color.R + 0.7152 * or_color.G + 0.0722 * or_color.B;

                        v += or_luminance;
                    }
                    finally
                    {
                    }
                }
            }
            double x_avgV = v / (x.Width * x.Height);

            v = 0;
            for (int i = 0; i < y.Width; i++)
            {
                for (int j = 0; j < y.Height; j++)
                {
                    try
                    {
                        Color or_color = y.GetPixel(i, j);
                        double or_luminance = 0.2126 * or_color.R + 0.7152 * or_color.G + 0.0722 * or_color.B;

                        v += or_luminance;
                    }
                    finally
                    {
                    }
                }
            }
            double y_avgV = v / (x.Width * x.Height);
            v = 0;
            for (int i = 0; i < x.Width; i++)
            {
                for (int j = 0; j < x.Height; j++)
                {
                    try
                    {
                        Color x_color = x.GetPixel(i, j);
                        double x_luminance = 0.2126 * x_color.R + 0.7152 * x_color.G + 0.0722 * x_color.B;

                        Color y_color = y.GetPixel(i, j);
                        double y_luminance = 0.2126 * y_color.R + 0.7152 * y_color.G + 0.0722 * y_color.B;

                        v += (x_luminance - x_avgV) * (y_luminance - y_avgV);
                    }
                    finally
                    {
                    }
                }
            }
            return v / (x.Height * x.Width);
        }

        private void ResetProcess()
        {
            if (task != null)
            {
                foreach (TaskGroup g in task.TaskGroups)
                {
                    g.Done = false;
                }
            }
        }
    }
}