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
        switch(this.unit) {
            case SpeedUnit.Kbit: 
                this.bytes *= 125;
                break;
            case SpeedUnit.Mbit: 
                this.bytes *= 125000;
                break;
        }

        let body: any = { "url": this.link, "threads": this.threads, "speed": this.bytes };

        var self = this;

        this.http.post(this.originUrl + '/api/Links', body).subscribe(result => {
            console.log(result);

            let source = new this.EventSource(this.originUrl + '/api/sse-notifications');
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
    }
}