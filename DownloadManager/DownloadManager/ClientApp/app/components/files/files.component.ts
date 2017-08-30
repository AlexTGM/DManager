import { Component, Inject } from '@angular/core';
import { Http } from '@angular/http';

@Component({
    selector: 'files',
    templateUrl: './files.component.html'
}) export class FilesComponent {
    public link : string;

    constructor(private http: Http, @Inject('ORIGIN_URL') private originUrl: string) { }

    public start() {
        let body: any = { "url": this.link, "threads": 8 }; 

        this.http.post(this.originUrl + '/api/Links', body).subscribe(result => {
            console.log(result);
        });
    }
}