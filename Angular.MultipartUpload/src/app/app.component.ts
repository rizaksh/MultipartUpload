import { Component } from '@angular/core';
import { HttpClient, HttpEvent, HttpEventType, HttpHeaders, HttpRequest, HttpResponse } from '@angular/common/http';
import { FormGroup, FormControl, Validators } from '@angular/forms';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})

export class AppComponent {
  preSignedUrlEndpoint: string = 'https://localhost:56149/api/fileupload/presignedurl';
  initiateUploadEndpoint: string = 'https://localhost:56149/api/fileupload/initiatemultipartupload';
  completeUploadEndpoint: string = 'https://localhost:56149/api/fileupload/completemultipartupload';
  files: any;
  

  myForm = new FormGroup({
    name: new FormControl('', [Validators.required, Validators.minLength(3)]),
    file: new FormControl('', [Validators.required]),
    fileSource: new FormControl('', [Validators.required])
  });

  constructor(private httpClient: HttpClient) { }

 
  onFileChange(event: any) {

    if (event.target.files.length > 0) {
      //const file = event.target.files[0];
      //this.myForm.patchValue({
      //  fileSource: file
      //});
      this.files = event.target.files;
    }
  }

  /**
   * Write code on Method
   *
   * @return response()
   */
  async submit() {
    //const formData = new FormData();
    //formData.append('file', this.myForm.get('fileSource')?.value);

    //this.httpClient.post('http://localhost:8001/upload.php', formData)
    //  .subscribe(res => {
    //    console.log(res);
    //    alert('Uploaded Successfully.');
    //  })
    if (!this.files || this.files.length  <1) {
      alert("No files selected. Select again.")
      return;
    }
    for (let i = 0; i < this.files.length; i++) {
      await this.uploadMultipartFile(this.files[i]);
    }
  }

  private getPreSignedUrl(filename: string, uploadId: string, index: number): Promise<any> {
    let request = {
      key: filename,
      type: "",//file.type,
      uploadId: uploadId,
      partNumber: index
    };

    let body = JSON.stringify(request);
    let options = {
      headers: new HttpHeaders({
        'content-type': 'application/json'
      })
    };
    return this.httpClient.post(this.preSignedUrlEndpoint, body, options).toPromise();
  }

  private initiateMultipartUpload(fileName:string, fileType:string): Promise<any> {

    let request = {
      key: fileName,
      type: fileType
    };

    let body = JSON.stringify(request);
    let options = {
      headers: new HttpHeaders({
        'content-type': 'application/json'
      })
    };

    return this.httpClient.post(this.initiateUploadEndpoint, body, options).toPromise();
  }

  async uploadMultipartFile(file: any) {
    let initiateMultipartResponse = await this.initiateMultipartUpload(file.name, file.type);

    try {
      const FILE_CHUNK_SIZE = 5000000; 
      const fileSize = file.size;
      const NUM_CHUNKS = Math.floor(fileSize / FILE_CHUNK_SIZE) + 1;
      let start, end, blob;

      let uploadPartsArray: { ETag: string; PartNumber: any; }[] = [];
      let countParts = 0;

      let orderData: { presignedUrl: any; index: number; }[] = [];

      for (let index = 1; index < NUM_CHUNKS + 1; index++) {
        start = (index - 1) * FILE_CHUNK_SIZE;
        end = (index) * FILE_CHUNK_SIZE;
        blob = (index < NUM_CHUNKS) ? file.slice(start, end) : file.slice(start);

        // (1) Generate presigned URL for each part
        let presignedUrl =  await this.getPreSignedUrl(file.name, initiateMultipartResponse.uploadId, index);

        // (2) Puts each file part into the storage server

        orderData.push({
          presignedUrl: presignedUrl.url.toString(),
          index: index
        });

        const req = new HttpRequest('PUT', presignedUrl.url.toString(), blob, {
          reportProgress: true
        });

        this.httpClient
          .request(req)
          .subscribe((event: HttpEvent<any>) => {
            switch (event.type) {
              case HttpEventType.UploadProgress:
                const percentDone = Math.round(100 * event.loaded / FILE_CHUNK_SIZE);
                //this.uploadProgress$.emit({
                //  progress: file.size < FILE_CHUNK_SIZE ? 100 : percentDone,
                //  token: tokenEmit
                //});
                break;
              case HttpEventType.Response:
                console.log('Done!');
            }

            // (3) Calls the CompleteMultipartUpload endpoint in the backend server

            if (event instanceof HttpResponse) {
              const currentPresigned = orderData.find(item => item.presignedUrl === event.url);

              countParts++;
              let etag  = "";
              if (event["headers"]) {

                let header = event["headers"];
                if (header.get('ETag')) {
                  let hValue = header.get('ETag');
                  if (hValue) {
                    etag = hValue;
                  }
                }
              }
              uploadPartsArray.push({
                ETag: etag,
                PartNumber: currentPresigned ? currentPresigned.index : ""
              });

              if (uploadPartsArray.length === NUM_CHUNKS) {
                let body = {
                  key: file.name,
                  partETags: uploadPartsArray.sort((a, b) => {
                    return a.PartNumber - b.PartNumber;
                  }),
                  uploadId: initiateMultipartResponse.uploadId
                }
                let options = {
                  headers: new HttpHeaders({
                    'content-type': 'application/json'
                  })
                };
                this.httpClient.post(this.completeUploadEndpoint, body, options).toPromise();
              }
            }
          });
      }
    } catch (e) {
      console.log('error: ', e);
    }
  }
}
