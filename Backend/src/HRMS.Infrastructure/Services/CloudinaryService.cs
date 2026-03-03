using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using HRMS.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace HRMS.Infrastructure.Services;

public class CloudinaryService : ICloudinaryService
{
    private readonly Cloudinary _cloudinary;

    public CloudinaryService(IConfiguration configuration)
    {
        var account = new Account(
            configuration["Cloudinary:CloudName"],
            configuration["Cloudinary:ApiKey"],
            configuration["Cloudinary:ApiSecret"]);
        _cloudinary = new Cloudinary(account);
    }

    public async Task<(string PublicId, string SecureUrl)> UploadAsync(Stream fileStream, string fileName, string folder)
    {
        var uploadParams = new RawUploadParams
        {
            File = new FileDescription(fileName, fileStream),
            Folder = folder,
            UseFilename = true,
            UniqueFilename = true,
            Overwrite = false
        };
        var result = await _cloudinary.UploadAsync(uploadParams);
        return (result.PublicId, result.SecureUrl.ToString());
    }

    public async Task<(string PublicId, string SecureUrl)> UploadPdfAsync(byte[] pdfBytes, string fileName, string folder)
    {
        using var stream = new MemoryStream(pdfBytes);
        return await UploadAsync(stream, fileName, folder);
    }

    public async Task DeleteAsync(string publicId)
    {
        await _cloudinary.DestroyAsync(new DeletionParams(publicId));
    }
}
