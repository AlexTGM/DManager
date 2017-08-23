import { Component, Inject } from '@angular/core';
import { Http } from '@angular/http';

@Component({
    selector: 'files',
    templateUrl: './files.component.html'
}) export class FilesComponent {
    public link : String;

    constructor(private http: Http, @Inject('ORIGIN_URL') private originUrl: string) { }

    public start() {
        this.http.get(this.originUrl + '/api/Links/LinkDownload').subscribe(result => {
        });
    }
}