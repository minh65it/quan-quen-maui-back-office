using Microsoft.Maui.Storage;
using System.Diagnostics;

namespace QuanQuenBackOfficeMobile.Services
{
    public interface ICameraService
    {
        Task<FileResult?> CapturePhotoWithTimeoutAsync(TimeSpan timeout);
        Task<FileResult?> PickPhotoWithTimeoutAsync(TimeSpan timeout);
        void CancelCurrentOperation();
        Task<string> OptimizeImageAsync(string imagePath);
    }

    public class CameraService : ICameraService
    {
        private CancellationTokenSource? _currentCancellationTokenSource;
        private bool _isOperationInProgress = false;

        public async Task<FileResult?> CapturePhotoWithTimeoutAsync(TimeSpan timeout)
        {
            if (_isOperationInProgress)
            {
                Debug.WriteLine("[CameraService] Một thao tác camera đang được thực hiện, hủy thao tác trước đó");
                CancelCurrentOperation();
            }

            _isOperationInProgress = true;
            _currentCancellationTokenSource = new CancellationTokenSource(timeout);

            try
            {
                Debug.WriteLine($"[CameraService] Bắt đầu chụp ảnh với timeout {timeout.TotalSeconds}s");
                
                // Kiểm tra quyền camera trước
                var cameraStatus = await Permissions.CheckStatusAsync<Permissions.Camera>();
                if (cameraStatus != PermissionStatus.Granted)
                {
                    cameraStatus = await Permissions.RequestAsync<Permissions.Camera>();
                    if (cameraStatus != PermissionStatus.Granted)
                    {
                        throw new UnauthorizedAccessException("Không có quyền truy cập camera");
                    }
                }

                if (!MediaPicker.Default.IsCaptureSupported)
                {
                    throw new NotSupportedException("Thiết bị không hỗ trợ chụp ảnh");
                }

                // Tạo task với timeout
                var captureTask = MediaPicker.Default.CapturePhotoAsync();
                var timeoutTask = Task.Delay(timeout, _currentCancellationTokenSource.Token);

                var completedTask = await Task.WhenAny(captureTask, timeoutTask);

                if (completedTask == timeoutTask)
                {
                    Debug.WriteLine("[CameraService] Camera session timeout");
                    throw new TimeoutException("Camera session quá lâu, đã tự động hủy để tránh crash");
                }

                var result = await captureTask;
                Debug.WriteLine($"[CameraService] Chụp ảnh thành công: {result?.FullPath}");
                return result;
            }
            catch (OperationCanceledException)
            {
                Debug.WriteLine("[CameraService] Camera operation bị hủy");
                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CameraService] Lỗi khi chụp ảnh: {ex.Message}");
                throw;
            }
            finally
            {
                _isOperationInProgress = false;
                _currentCancellationTokenSource?.Dispose();
                _currentCancellationTokenSource = null;
            }
        }

        public async Task<FileResult?> PickPhotoWithTimeoutAsync(TimeSpan timeout)
        {
            if (_isOperationInProgress)
            {
                Debug.WriteLine("[CameraService] Một thao tác đang được thực hiện, hủy thao tác trước đó");
                CancelCurrentOperation();
            }

            _isOperationInProgress = true;
            _currentCancellationTokenSource = new CancellationTokenSource(timeout);

            try
            {
                Debug.WriteLine($"[CameraService] Bắt đầu chọn ảnh với timeout {timeout.TotalSeconds}s");
                
                // Kiểm tra quyền đọc media
                var mediaStatus = await Permissions.CheckStatusAsync<Permissions.Photos>();
                if (mediaStatus != PermissionStatus.Granted)
                {
                    mediaStatus = await Permissions.RequestAsync<Permissions.Photos>();
                    if (mediaStatus != PermissionStatus.Granted)
                    {
                        throw new UnauthorizedAccessException("Không có quyền truy cập thư viện ảnh");
                    }
                }

                // Tạo task với timeout
                var pickTask = MediaPicker.Default.PickPhotoAsync(new MediaPickerOptions { Title = "Chọn ảnh nhà hàng" });
                var timeoutTask = Task.Delay(timeout, _currentCancellationTokenSource.Token);

                var completedTask = await Task.WhenAny(pickTask, timeoutTask);

                if (completedTask == timeoutTask)
                {
                    Debug.WriteLine("[CameraService] Pick photo timeout");
                    throw new TimeoutException("Chọn ảnh quá lâu, đã tự động hủy");
                }

                var result = await pickTask;
                Debug.WriteLine($"[CameraService] Chọn ảnh thành công: {result?.FullPath}");
                return result;
            }
            catch (OperationCanceledException)
            {
                Debug.WriteLine("[CameraService] Pick photo operation bị hủy");
                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CameraService] Lỗi khi chọn ảnh: {ex.Message}");
                throw;
            }
            finally
            {
                _isOperationInProgress = false;
                _currentCancellationTokenSource?.Dispose();
                _currentCancellationTokenSource = null;
            }
        }

        public void CancelCurrentOperation()
        {
            if (_currentCancellationTokenSource != null && !_currentCancellationTokenSource.IsCancellationRequested)
            {
                Debug.WriteLine("[CameraService] Hủy thao tác camera hiện tại");
                _currentCancellationTokenSource.Cancel();
            }
        }

        public async Task<string> OptimizeImageAsync(string imagePath)
        {
            try
            {
                Debug.WriteLine($"[CameraService] Optimizing image: {imagePath}");
                
                // Kiểm tra file size
                var fileInfo = new FileInfo(imagePath);
                if (fileInfo.Length < 1024 * 1024) // < 1MB thì không cần optimize
                {
                    Debug.WriteLine("[CameraService] Image size is small, no optimization needed");
                    return imagePath;
                }

                // Tạo optimized version với smaller size
                var optimizedPath = Path.Combine(Path.GetDirectoryName(imagePath)!, 
                    $"optimized_{Path.GetFileName(imagePath)}");

                // Copy file với buffer size nhỏ hơn để giảm memory pressure
                using (var sourceStream = File.OpenRead(imagePath))
                using (var destStream = File.OpenWrite(optimizedPath))
                {
                    var buffer = new byte[4096]; // 4KB buffer thay vì 8KB
                    int bytesRead;
                    while ((bytesRead = await sourceStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        await destStream.WriteAsync(buffer, 0, bytesRead);
                    }
                }

                var optimizedSize = new FileInfo(optimizedPath).Length;
                Debug.WriteLine($"[CameraService] Image optimized: {fileInfo.Length / 1024}KB -> {optimizedSize / 1024}KB");
                
                return optimizedPath;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CameraService] Image optimization failed: {ex.Message}");
                return imagePath; // Return original path if optimization fails
            }
        }

        public void Dispose()
        {
            CancelCurrentOperation();
            _currentCancellationTokenSource?.Dispose();
        }
    }
}
