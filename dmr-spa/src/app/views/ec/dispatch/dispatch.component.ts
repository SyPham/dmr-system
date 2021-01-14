import { Component, Input, OnInit, ViewChild } from '@angular/core';
import { HubConnectionState } from '@microsoft/signalr';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { GridComponent } from '@syncfusion/ej2-angular-grids';
import { IBuilding } from 'src/app/_core/_model/building.js';
import { IToDoList } from 'src/app/_core/_model/IToDoList.js';
import { DispatchParams, IDispatch, IDispatchForCreate } from 'src/app/_core/_model/plan';
import { IRole } from 'src/app/_core/_model/role.js';
import { AbnormalService } from 'src/app/_core/_service/abnormal.service.js';
import { AlertifyService } from 'src/app/_core/_service/alertify.service.js';
import { DispatchService } from 'src/app/_core/_service/dispatch.service.js';
import { IngredientService } from 'src/app/_core/_service/ingredient.service.js';
import { MakeGlueService } from 'src/app/_core/_service/make-glue.service.js';
import { PlanService } from 'src/app/_core/_service/plan.service.js';
import { SettingService } from 'src/app/_core/_service/setting.service.js';
import { TodolistService } from 'src/app/_core/_service/todolist.service.js';
import * as signalr from '../../../../assets/js/ec-client.js';
const UNIT_SMALL_MACHINE = 'g';
@Component({
  selector: 'app-dispatch',
  templateUrl: './dispatch.component.html',
  styleUrls: ['./dispatch.component.css']
})
export class DispatchComponent implements OnInit {
  @ViewChild('dispatchGrid')
  dispatchGrid: GridComponent;
  @Input() value: IToDoList;
  @Input() buildingSetting: any;
  toolbarOptions: any;
  filterSettings: { type: string; };
  fieldsLine: object = { text: 'line', value: 'line' };
  editSettings = { showDeleteConfirmDialog: false, allowEditing: true, allowAdding: false, allowDeleting: false, mode: 'Normal' };
  setFocus: any;
  title: string;
  data: IDispatch[];
  buildingID: number;
  role: IRole;
  building: IBuilding;
  startDispatchingTime: any;
  finishDispatchingTime: any;
  mixedConsumption: number;
  unitTitle: string;
  line: any;
  qrCode: string;
  user: any;
  isShow: boolean;
  constructor(
    public activeModal: NgbActiveModal,
    public settingService: SettingService,
    public dispatchService: DispatchService,
    public planService: PlanService,
    public todolistService: TodolistService,
    public alertify: AlertifyService

  ) { }

  ngOnInit() {
    this.toolbarOptions = [
      'Edit', 'Cancel', 'Search'];
    this.title = this.value.glueName;
    const BUIDLING: IBuilding = JSON.parse(localStorage.getItem('building'));
    const ROLE: IRole = JSON.parse(localStorage.getItem('level'));
    const USER = JSON.parse(localStorage.getItem('user')).User;
    this.role = ROLE;
    this.user = USER;
    this.building = BUIDLING;
    if (this.value.glueName.includes(' + ')) {
      this.unitTitle = 'Actual Consumption';
      this.mixedConsumption = +(this.value.mixedConsumption * 1000).toFixed(0);
    } else {
      this.unitTitle = 'Standard Consumption';
      this.mixedConsumption = +(this.value.standardConsumption * 1000).toFixed(0);
    }
    this.loadData();
    this.startDispatchingTime = new Date();
    this.isShow = this.value.mixingInfoID === 0 && !this.value.glueName.includes(' + ');
  }
  actionBegin(args) {
    if (args.requestType === 'save' && args.action === 'edit') {
      // const check = this.mixedConsumption + data.real - data.real
      const previousData = args.previousData as IDispatch;
      const data = args.data as IDispatch;
      const check = (previousData.real + this.mixedConsumption) - data.real;
      if (check < 0 && this.value.glueName.includes(' + ')) {
        this.alertify.warning('Không được thêm quá số gram!', true);
        this.dispatchGrid.refresh();
        return;
      }
      this.update(data);
    }
  }
  actionComplete(e){
    if (e.requestType === 'beginEdit') {
      const input = (e.form.elements.namedItem('real') as HTMLInputElement);
      input.focus();
      if (input.value === '0') {
        input.select();
      }
    }
  }
  toolbarClick(args) {
  }
  loadData() {
    const obj: DispatchParams = {
      id: this.value.id,
      glue: this.value.glueName,
      lines: this.value.lineNames,
      mixingInfoID: this.value.mixingInfoID,
      estimatedTime: this.value.estimatedFinishTime,
      estimatedStartTime: this.value.estimatedStartTime,
      estimatedFinishTime: this.value.estimatedFinishTime,
    };
    this.todolistService.dispatch(obj).subscribe(data => {
      this.data = data.map(item => {
        const itemData: IDispatch = {
          id: item.id,
          lineID: item.lineID,
          line: item.line,
          standardAmount: item.standardAmount,
          mixingInfoID: item.mixingInfoID,
          mixedConsumption: item.mixedConsumption,
          glue: item.glue,
          stationID: item.stationID,
          real: item.real * 1000,
          warningStatus: false,
          scanStatus: false,
          isLock: false,
          isNew: false,
          createdTime: item.createdTime,
          deliveryTime: item.deliveryTime
        };
        return itemData;
      });
      if (this.value.glueName.includes(' + ')) {
        this.unitTitle = 'Actual Consumption';
        this.mixedConsumption = +(this.value.mixedConsumption * 1000).toFixed(0);
      } else {
        this.unitTitle = 'Standard Consumption';
        this.mixedConsumption = +(this.value.standardConsumption * 1000).toFixed(0);
      }
      const deliverTotal = this.data.reduce((a, b) => a + (b.real || 0), 0);
      const res = this.mixedConsumption - deliverTotal;
      this.mixedConsumption = res ;
      let id = 0;
      const dataID = this.data.map(item => item.id);
      id = Math.min(...dataID);
      this.updateStartDispatchingTime(id);
    });
  }
  add(data: IDispatch) {
    const obj: IDispatchForCreate = {
      id: 0,
      mixingInfoID: data.mixingInfoID,
      lineID: data.lineID,
      amount: data.real / 1000,
      createdTime: new Date(),
      stationID: data.stationID,
      standardAmount: this.value.standardConsumption,
      estimatedTime: this.value.estimatedFinishTime,
      startDispatchingTime: this.startDispatchingTime,
      finishDispatchingTime: this.finishDispatchingTime
    };
    this.dispatchService.add(obj).subscribe((res) => {
      this.loadData();
      this.alertify.success('Success');
    }, error => {
        this.alertify.warning('error');
    });
  }
  update(data: IDispatch) {
    this.dispatchService.updateAmount(data.id, data.real / 1000).subscribe((res) => {
      this.loadData();
      this.alertify.success('Success');
    }, error => {
      this.alertify.warning('error');
    });
  }
  addDispatch() {
    const obj: IDispatchForCreate[] = this.data.map(data => {
        const item: IDispatchForCreate = {
          id: data.id,
          mixingInfoID: data.mixingInfoID,
          lineID: data.lineID,
          amount: data.real / 1000,
          createdTime: new Date(),
          stationID: data.stationID,
          standardAmount: this.value.standardConsumption,
          estimatedTime: this.value.estimatedFinishTime,
          startDispatchingTime: this.startDispatchingTime,
          finishDispatchingTime: this.finishDispatchingTime
        };
        return item;
    });
    this.dispatchService.addDispatch(obj).subscribe((res: any) => {
      if (res.status) {
        this.loadData();
        this.alertify.success('Success');
        this.todolistService.setValue(true);
        this.activeModal.dismiss();
      } else {
        this.alertify.warning(res.message, true);
      }
    }, error => {
        this.alertify.warning(error);
    });
  }
  updateStartDispatchingTime(id: number) {
    this.dispatchService.updateStartDispatchingTime(id).subscribe((res) => {
    }, error => {
        this.alertify.warning(error);
    });
  }
  save() {
    this.addDispatch();
  }
}
