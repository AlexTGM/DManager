import { Component, Inject, NgZone } from '@angular/core';
import { Http } from '@angular/http';

class FileInfo {
    name: string;
    progress: string;
}

enum SpeedUnit {
    Mbit = 0,
    Kbit = 1
}

@Component({
    selector: 'files',
    templateUrl: './files.component.html'
})
export class FilesComponent {
    public link: string;
    public threads: number;
    public bytes: number;
    public unit: SpeedUnit = 0;

    files: string;
    files1: string[] = [];

    speed: any = "0 mbits";

    EventSource: any = window['EventSource'];

    constructor(private zone: NgZone, private http: Http, @Inject('ORIGIN_URL') private originUrl: string) {
    }

    public start() {
        var speedLimit = 0;

        switch(this.unit) {
            case SpeedUnit.Kbit: 
                speedLimit = this.bytes * 125;
                break;
            case SpeedUnit.Mbit: 
                speedLimit = this.bytes * 125000;
                break;
        }

        let source = new this.EventSource(this.originUrl + '/api/sse-notifications');
        
        let body: any = { "url": this.link, "threads": this.threads, "speed": speedLimit };

        var self = this;
        
        this.http.post(this.originUrl + '/api/Links', body).subscribe(result => {
            console.log(result);
            source.onmessage = event => {
                if (event.lastEventId === "speed") {
                    this.zone.run(() =>
                        self.speed = event.data * 8e-6 + ' mbits');
                } else {
                    this.zone.run(() => {
                        self.files1[event.lastEventId] = event.data;

                        self.files = "";

                        for (var i in self.files1)
                            self.files += i + " / " + self.files1[i] + "\n";
                    });
                }
            };
        });

        this.link = "";
        this.threads = 1;
        this.bytes = 0;
    }
}