using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AWSLamba.MultipartUpload.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileUploadController : ControllerBase
    {
        private readonly IAmazonS3 amazonS3;
        private readonly string bucketName;

        public FileUploadController(IAmazonS3 amazonS3, IConfiguration configuration)
        {
            this.amazonS3 = amazonS3;
            this.bucketName = configuration["AWS:S3:BucketName"];
        }

        [HttpGet]
        [Route("test")]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        /// <summary>
        /// Returns the Presigned Url for the given bucketname, filename, contenttype, uploadid and partnumber
        /// </summary>
        /// <param name="getPreSignedUrlRequest"><see cref="GetPreSignedUrlRequest"/></param>
        /// <returns></returns>
        [HttpPost]
        [Route("presignedurl")]
        public IActionResult PreSignedUrl([FromBody] GetPreSignedUrlRequest getPreSignedUrlRequest)
        {
            getPreSignedUrlRequest.BucketName = bucketName;
            getPreSignedUrlRequest.Expires = DateTime.Now.AddSeconds(30);
            getPreSignedUrlRequest.Verb = HttpVerb.PUT;
            var signedUrl = amazonS3.GetPreSignedURL(getPreSignedUrlRequest);
            return Ok(new { url = signedUrl });
        }


        /// <summary>
        /// initiates a multipart upload and returns an upload ID 
        /// </summary>
        /// <param name="initiateMultipartUploadRequest"><see cref="InitiateMultipartUploadRequest"/></param>
        /// <returns></returns>
        [HttpPost]
        [Route("initiatemultipartupload")]
        public async Task<IActionResult> InitiateMultipartUploadAsync([FromBody] InitiateMultipartUploadRequest initiateMultipartUploadRequest)
        {
            // Add model validations for required fields - Bucketname, Key, ContentType 
            initiateMultipartUploadRequest.BucketName = bucketName;
            var initiateMultipartUploadResponse = await amazonS3.InitiateMultipartUploadAsync(initiateMultipartUploadRequest);
            return Ok(new { uploadId = initiateMultipartUploadResponse.UploadId });
        }

        /// <summary>
        /// Completes a multipart upload of given upload id and part tags.
        /// </summary>
        /// <param name="completeMultipartUploadRequest"><see cref="CompleteMultipartUploadRequest"/></param>
        /// <returns></returns>
        [HttpPost]
        [Route("completemultipartupload")]
        public async Task<IActionResult> CompleteMultipartUploadAsync([FromBody] CompleteMultipartUploadRequest completeMultipartUploadRequest)
        {
            completeMultipartUploadRequest.BucketName = bucketName;
            // Add model validations for required fields - Bucketname, Key, UploadId, PartEtags
            var completeMultipartUploadResponse = await amazonS3.CompleteMultipartUploadAsync(completeMultipartUploadRequest);
            return Ok(completeMultipartUploadResponse);
        }

        /// <summary>
        /// Abort multi part upload for the given key and upload id
        /// </summary>
        /// <param name="abortMultipartUploadRequest"><see cref="AbortMultipartUploadRequest"/></param>
        /// <returns></returns>
        [HttpPost]
        [Route("abortmultipartupload")]
        public async Task<IActionResult> AbortMulitpartUploadAsync([FromBody] AbortMultipartUploadRequest abortMultipartUploadRequest)
        {
            abortMultipartUploadRequest.BucketName = bucketName;
            // Add model validations for required fields - Bucketname, Key, UploadId
            var abortMultipartUploadResponse = await amazonS3.AbortMultipartUploadAsync(abortMultipartUploadRequest);
            return Ok(abortMultipartUploadResponse.ResponseMetadata);
        }
    }
}
