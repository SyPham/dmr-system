import { Component, OnInit, ViewChild, TemplateRef, ViewChildren, QueryList } from '@angular/core';
import { ModalNameService } from 'src/app/_core/_service/modal-name.service';
import { AlertifyService } from 'src/app/_core/_service/alertify.service';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { GridComponent, ExcelExportProperties, Column } from '@syncfusion/ej2-angular-grids';
import { BuildingUserService } from 'src/app/_core/_service/building.user.service';
import { environment } from '../../../../environments/environment';
import { BPFCEstablishService } from 'src/app/_core/_service/bpfc-establish.service';
import { DatePipe } from '@angular/common';
import { UserService } from 'src/app/_core/_service/user.service';
import { Router } from '@angular/router';

@Component({
  // tslint:disable-next-line:component-selector
  selector: 'app-BPFCSchedule',
  templateUrl: './BPFCSchedule.component.html',
  styleUrls: ['./BPFCSchedule.component.css'],
  providers: [DatePipe]
})
export class BPFCScheduleComponent implements OnInit {
  @ViewChildren('tooltip') tooltip: QueryList<any>;

  pageSettings = { pageCount: 20, pageSizes: [50, 100, 150, 200, 'All'], pageSize: 50 };
  data: any[];
  editSettings: object;
  toolbar: object;
  file: any;

  @ViewChild('grid')
  public gridObj: GridComponent;
  modalReference: NgbModalRef;
  @ViewChild('importModal', { static: true })
  importModal: TemplateRef<any>;
  excelDownloadUrl: string;
  users: any[];
  filterSettings: { type: string; };
  modelName: string = null;
  modelNo: string = null;
  articleNo: string = null;
  articleNoNew: string = null;
  artProcess: string = null;
  modelNameID = 0;
  modelNoID = 0;
  articleNoID = 0;
  artProcessID = 0;
  BPFCID = 0;
  constructor(
    private modalNameService: ModalNameService,
    private alertify: AlertifyService,
    private userService: UserService,
    private bPFCEstablishService: BPFCEstablishService,
    public modalService: NgbModal,
    private router: Router,
    private datePipe: DatePipe
  ) { }

  ngOnInit() {
    this.excelDownloadUrl = `${environment.apiUrlEC}ModelName/ExcelExport`;
    this.toolbar = ['Excel Import', 'ExcelExport', 'Search'];
    this.filterSettings = { type: 'Excel' };
    this.editSettings = { allowEditing: true, allowAdding: true, allowDeleting: true, newRowPosition: 'Normal' };
    this.getAllUsers();
  }
  onBeforeRender(args, data, i) {
    const t = this.tooltip.filter((item, index) => index === +i)[0];
    t.content = 'Loading...';
    t.dataBind();
    this.bPFCEstablishService
      .getGlueByBPFCID(data.id)
      .subscribe((res: any) => {
        t.content = res.join('<br>');
        t.dataBind();
      });
  }

  onClickClone() {
    const clone = {
      modelNameID: this.modelNameID,
      modelNOID: this.modelNoID,
      articleNOID: this.articleNoID,
      artProcessID: Number(this.artProcessID),
      bpfcID: this.BPFCID,
      name: this.articleNoNew,
      cloneBy: JSON.parse(localStorage.getItem('user')).User.ID,
    };

    this.clone(clone);
  }
  clone(clone) {
    if (this.articleNoNew !== this.articleNo) {
      this.modalNameService.cloneBPFC(clone).subscribe((res: any) => {
        if (res.status === true) {
          this.alertify.success('Đã sao chép thành công!');
          this.modalService.dismissAll();
          this.getAllUsers();
        } else {
          this.alertify.error('The BPFC exists!');
        }
      });
    } else {
      this.alertify.error('The BPFC exists!');
    }
  }
  openModal(ref, data) {
    this.modalReference = this.modalService.open(ref);
    this.BPFCID = data.id;
    this.modelName = data.modelName;
    this.modelNo = data.modelNo;
    this.articleNo = data.articleNo;
    this.artProcess = data.artProcess;
    this.articleNoNew = data.articleNo;

    this.modelNameID = data.modelNameID;
    this.modelNoID = data.modelNoID;
    this.articleNoID = data.articleNoID;
    if (data.artProcess === 'ASY') {
      this.artProcessID = 1;
    } else {
      this.artProcessID = 2;
    }
  }
  detail(data) {
    return this.router.navigate([`/ec/establish/bpfc-schedule/detail/${data.id}`]);
  }
  actionBegin(args) {
    console.log(args.requestType + ' ' + args.type); // custom Action
    if (args.requestType === 'save') {
      const entity = {
        id: args.data.id,
        season: args.data.season
      };
      this.bPFCEstablishService.updateSeason(entity).subscribe(() => {
        this.alertify.success('Update Season Success');
        this.getAllUsers();
      });
    }
  }
  dataBound() {
   this.gridObj.autoFitColumns();
  }
  toolbarClick(args) {
    switch (args.item.text) {
      case 'Excel Import':
        this.showModal(this.importModal);
        break;
      case 'Excel Export':
        const data = this.data.map(item => {
          return {
            approvedBy: item.approvedBy,
            approvalStatus: item.approvalStatus,
            createdBy: item.createdBy,
            articleNo: item.articleNo,
            createdDate: this.datePipe.transform(item.createdDate, 'd MMM, yyyy HH:mm'),
            artProcess: item.artProcess,
            finishedStatus: item.finishedStatus === true ? 'Yes' : 'No',
            modelName: item.modelName,
            modelNo: item.modelNo,
            season: item.season
          };
        });
        const exportProperties = {
          dataSource: data
        };
        this.gridObj.excelExport(exportProperties);
        break;
    }
  }

  fileProgress(event) {
    this.file = event.target.files[0];
  }

  uploadFile() {
    const createdBy = JSON.parse(localStorage.getItem('user')).User.ID;
    this.bPFCEstablishService.import(this.file, createdBy)
    .subscribe((res: any) => {
      this.getAll();
      this.modalReference.close();
      this.alertify.success('The excel has been imported into system!');
    });
  }


  getAllUsers() {
    this.userService.getAllUserInfo().subscribe((res: any) => {
      this.users = res;
      this.getAll();
    });
  }

  getAll() {
    this.bPFCEstablishService.getAll().subscribe( (res: any) => {
      this.data = res.map( (item: any) => {
        return {
          id: item.id,
          modelNameID: item.modelNameID,
          modelNoID: item.modelNoID,
          articleNoID: item.articleNoID,
          artProcessID: item.artProcessID,
          modelName: item.modelName,
          modelNo: item.modelNo,
          createdDate: new Date(item.createdDate),
          articleNo: item.articleNo,
          approvalStatus: item.approvalStatus,
          finishedStatus: item.finishedStatus,
          approvedBy: this.users.filter(a => a.id === item.approvalBy)[0]?.username,
          createdBy: item.createdBy,
          artProcess: item.artProcess,
          season: item.season
        };
      });
    });
  }

  showModal(importModal) {
    this.modalReference = this.modalService.open(importModal, { size: 'xl' });
  }
  NO(index) {
    return (this.gridObj.pageSettings.currentPage - 1) * this.gridObj.pageSettings.pageSize + Number(index) + 1;
  }
}
