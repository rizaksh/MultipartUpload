# Uploading large files to private S3 through SPA using presigned url C# #

A reference implementation of multipart file upload to private S3 through SPA using presigned without exposing AWS Credentials in SPA application.

## Overview ##

### Application ###

This solution has two projects
  1. Angular.MultipartUpload - A SPA angluar application to upload large files to S3 bucket.
  2. AWSLambda.MultipartUpload - An AWS Lambda exposed as webapi endpoints, to communicate with private S3 bucket for initiating mulipart upload, generate presigned url, complete mulitpart upload or abort multipart upload.

### S3 Multipart ###

With Multipart Upload in S3, you can upload a single object as a collection of parts using multipart upload. Each component is a continuous chunk of data for the object. 

These object parts can be uploaded separately and in any order. After each component of your object is uploaded, Amazon S3 puts everything together to produce the final object.

Multipart uploads is recommended for file size beyond 100 MB.

	

### S3 PreSigned Url ###

By default, all objects and buckets are private. You can, however, use a presigned URL to share files or permit your users or customers to upload objects to buckets without using their AWS security credentials or permissions.


## Upload process ##
The upload process in this solution is a 5 steps process. 

1. Start the process with InitiateMultipartUpload which returns a unique UploadId, all further steps will reference this upload id. 
2. Split the large files into part of atleast 5 MB in size and no of part not more than 10000.
3. Generate a presigned url using GetPresingedUrl, for each part.
4. Using presigned Url Upload the part with UploadId and part number, a part number uniquely identifies a part and its position in the object you are uploading. 
For each part upload, a response with Etag is received.
5. Once all parts are uploaded to S3, complete the upload with CompleteMultipartUpload using the UploadId, Etags and partnumbers.

### Configuration ###

#### S3 Bucket ####

1.	CORS policy of S3 bucket has to be updated to allow atleast the following in Origin, AllowedMethods and ExposeHeaders

	eg.
	[
		{
			"AllowedHeaders": [
				"*"
			],
			"AllowedMethods": [
				"PUT"
			],
			"AllowedOrigins": [
				"http://localhost:4200"
			],
			"ExposeHeaders": [
				"Etag"
			]
		}
	]

2.	Configure the bucket lifecycle policy to abort incomplete multipart uploads.


#### AWSLamba.MultipartUpload  ####

Update your bucket name in appsetting.json.
Also, if required, make changes to AWSOptions in Startup.cs

## Running the solution ##

Prerequisite - .net 6, Node.js and npm

To run the solution in local you need to run the API project and Angular project, instructions follows

1. Api project 
		Open CMD, change directory to MultipartUpload\AWSLamba.MultipartUpload\src\AWSLamba.MultipartUpload and run comamnd "dotnet run".
2. Api
		a. Open CMD, change directory to MultipartUpload\Angular.MultipartUpload and run command "npm install".
		b. Once npm install is complete, run "ng serve"
	   
