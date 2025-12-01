using MedicalImageProcessor.Core.Entities;

namespace MedicalImageProcessor.Core.Interfaces
{
    public interface IImageProcessor
    {
        Task<byte[]> PreprocessAsync(ImageInput input, CancellationToken cancellationToken = default);
    }
}