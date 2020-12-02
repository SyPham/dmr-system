import { DatePipe } from '@angular/common';
import { Component, Input, OnInit } from '@angular/core';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { DisplayTextModel } from '@syncfusion/ej2-angular-barcode-generator';
import { DispatchParams, IMixingInfo, Todolist } from 'src/app/_core/_model/plan';
import { AlertifyService } from 'src/app/_core/_service/alertify.service';
import { PlanService } from 'src/app/_core/_service/plan.service';

@Component({
  selector: 'app-print-glue',
  templateUrl: './print-glue.component.html',
  styleUrls: ['./print-glue.component.css']
})
export class PrintGlueComponent implements OnInit {
  @Input() data: any;
  public displayTextMethod: DisplayTextModel = {
    visibility: false
  };
  constructor(
    public activeModal: NgbActiveModal,
    public planService: PlanService,
    public alertify: AlertifyService,
    private datePipe: DatePipe,

  ) { }

  ngOnInit() {
  }
  printData() {

    let html = '';
    const printContent = document.getElementById('qrcode');

    // tslint:disable-next-line:max-line-length
    const exp = this.data.expiredTime === '0001-01-01T00:00:00' ? 'N/A' : this.datePipe.transform(new Date(this.data.expiredTime), 'yyyyMMdd HH:mm');
    html += `
       <div class='content'>
        <div class='qrcode'>
         ${printContent.innerHTML}
         </div>
          <div class='info'>
          <ul>
              <li class='subInfo'>Name: ${this.data.glue}</li>
              <li class='subInfo'>QR Code: ${this.data.code}</li>
              <li class='subInfo'>Batch: ${this.data.batchA}</li>
              <li class='subInfo'>MFG: ${this.datePipe.transform(new Date(this.data.createdTime), 'yyyyMMdd HH:mm')}</li>
              <li class='subInfo'>EXP: ${exp}</li>
          </ul>
         </div>
      </div>
      `;
    this.configurePrint(html);

  }
  configurePrint(html) {
    const WindowPrt = window.open('', '_blank', 'left=0,top=0,width=1000,height=900,toolbar=0,scrollbars=0,status=0');
    // WindowPrt.document.write(printContent.innerHTML);
    WindowPrt.document.write(`
    <html>
      <head>
      </head>
      <style>
          body {
        width: 100%;
        height: 100%;
        margin: 0;
        padding: 0;
        background-color: #FAFAFA;
        font: 12pt "Tahoma";
    }
    * {
        box-sizing: border-box;
        -moz-box-sizing: border-box;
    }
    .page {
        width: 210mm;
        min-height: 297mm;
        padding: 20mm;
        margin: 10mm auto;
        border: 1px #D3D3D3 solid;
        border-radius: 5px;
        background: white;
        box-shadow: 0 0 5px rgba(0, 0, 0, 0.1);
    }
    .subpage {
        padding: 1cm;
        border: 5px red solid;
        height: 257mm;
        outline: 2cm #FFEAEA solid;
    }
     .content {
        height: 221px;
        display:block;
        clear:both;
         border: 1px #D3D3D3 solid;
    }
    .content .qrcode {
      float:left;

      width: 177px;
      height:177px;
      margin-top: 12px;
      margin-bottom: 12px;
      margin-left: 5px;
       border: 1px #D3D3D3 solid;
    }
    .content .info {
       float:left;
       list-style: none;
    }
    .content .info ul {
       float:left;
       list-style: none;
       padding: 0;
       margin: 0;
      margin-top: 25px;
    }
    .content .info ul li.subInfo {
      padding: .20rem .75rem;
    }
    @page {
        size: A4;
        margin: 0;
    }
    @media print {
        html, body {
            width: 210mm;
            height: 297mm;
        }
        .page {
            margin: 0;
            border: initial;
            border-radius: initial;
            width: initial;
            min-height: initial;
            box-shadow: initial;
            background: initial;
            page-break-after: always;
        }
    }
      </style>
      <body onload="window.print(); window.close()">
        ${html}
      </body>
    </html>
    `);
    WindowPrt.document.close();
    this.finish();
  }
  finish() {
    this.planService.finish(this.data.id).subscribe((data: any) => {
      this.alertify.success('success' + data, true);
      this.planService.setValue(true);
    });
  }
}
