import { Component, Input, OnInit, ViewChild } from '@angular/core';
import { HubConnectionState } from '@microsoft/signalr';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { IBuilding } from 'src/app/_core/_model/building.js';
import { DispatchParams, IDispatch, IDispatchForCreate, Todolist } from 'src/app/_core/_model/plan';
import { IRole } from 'src/app/_core/_model/role.js';
import { AlertifyService } from 'src/app/_core/_service/alertify.service.js';
import { DispatchService } from 'src/app/_core/_service/dispatch.service.js';
import { PlanService } from 'src/app/_core/_service/plan.service.js';
import { SettingService } from 'src/app/_core/_service/setting.service.js';
import * as signalr from '../../../../assets/js/ec-client.js';
const UNIT_SMALL_MACHINE = 'g';
@Component({
  selector: 'app-dispatch',
  templateUrl: './dispatch.component.html',
  styleUrls: ['./dispatch.component.css']
})
export class DispatchComponent implements OnInit {
  @Input() value: Todolist;
  @Input() buildingSetting: any;

  title: string;
  data: IDispatch[];
  buildingID: number;
  scalingSetting: any;
  amount: number;
  role: IRole;
  index: number;
  building: IBuilding;
  indexesArray: number[] = [];
  constructor(
    public activeModal: NgbActiveModal,
    public settingService: SettingService,
    public dispatchService: DispatchService,
    public planService: PlanService,
    public alertify: AlertifyService,

  ) { }

  ngOnInit() {
    this.title = this.value.glue;
    const BUIDLING: IBuilding = JSON.parse(localStorage.getItem('building'));
    const ROLE: IRole = JSON.parse(localStorage.getItem('level'));
    this.role = ROLE;
    this.building = BUIDLING;
    this.indexesArray = [];

    this.getScalingSetting();
    this.loadData();
    this.index = 0;
  }
  onFocus(args, index, data) {
    if (data.id > 0) {
      this.alertify.error('Đã có dữ liệu', true);
      return;
    }
    this.index = index;
    this.onSignalr();
    this.signal(index);
  }
  keytab(args) {
    this.offSignalr();
  }
  private offSignalr() {
    signalr.SCALING_CONNECTION_HUB.off('Welcom');
  }
  private onSignalr() {
    signalr.SCALING_CONNECTION_HUB.on('Welcom');
  }
  private detechAmoutEmpty() {
    this.indexesArray = [];
    for (const key in this.data) {
      if (Object.prototype.hasOwnProperty.call(this.data, key)) {
        const element = this.data[key];
        if (element.id === 0) {
          this.indexesArray.push(+key);
        }
      }
    }
    console.log(this.indexesArray);
  }
  signalAuto() {
    const index = this.indexesArray[0];
    if (this.indexesArray.length === 0) {  this.planService.setValue(true); return; }
    if (signalr.SCALING_CONNECTION_HUB.state === HubConnectionState.Connected) {
      signalr.SCALING_CONNECTION_HUB.on(
        'Welcom',
        (scalingMachineID, message, unit) => {
          if (this.scalingSetting.includes(+scalingMachineID)) {
            if (unit === UNIT_SMALL_MACHINE) {
              this.amount = parseFloat(message);
              const item = this.data[index];
              const gram = (item.standardAmount * 1000);
              const allowance = gram * 0.02;
              const min = gram - allowance;
              const max = gram + allowance;
              if (this.amount >= min && this.amount <= max) {
                this.data[index].real = parseFloat(message);
                this.data[index].warningStatus = false;
                this.add(item);
                this.offSignalr();
                return;
              } else {
                this.data[index].real = parseFloat(message);
                this.data[index].warningStatus = true;
              }
            }
          }
        }
      );
    } else {
      // this.signalRService.connect();
    }
  }
  signal(index) {
    if (signalr.SCALING_CONNECTION_HUB.state === HubConnectionState.Connected) {
      signalr.SCALING_CONNECTION_HUB.on(
        'Welcom',
        (scalingMachineID, message, unit) => {
          if (this.scalingSetting.includes(+scalingMachineID)) {
            if (unit === UNIT_SMALL_MACHINE) {
              this.amount = parseFloat(message);
              const item = this.data[index];
              const gram = (item.standardAmount * 1000);
              const allowance = gram * 0.02;
              const min = gram - allowance;
              const max = gram + allowance;
              if (this.amount >= min && this.amount <= max) {
                this.data[index].real = parseFloat(message);
                this.data[index].warningStatus = false;
                this.add(item);
                this.offSignalr();
                return;
              } else {
                this.data[index].real = parseFloat(message);
                this.data[index].warningStatus = true;
              }
            }
          }
        }
      );
    } else {
      this.startScalingHub();
    }
  }
  private startScalingHub() {
    signalr.SCALING_CONNECTION_HUB.start().then( () =>  {
      signalr.SCALING_CONNECTION_HUB.on('Scaling Hub UserConnected', (conId) => {
        console.log('Scaling Hub UserConnected', conId);
      });
      signalr.SCALING_CONNECTION_HUB.on('Scaling Hub User Disconnected', (conId) => {
        console.log('Scaling Hub User Disconnected', conId);
      });
      console.log('Scaling Hub Signalr connected');
    }).catch((err) => {
      setTimeout(() => this.startScalingHub(), 5000);
    });
  }
  private getScalingSetting() {
    this.buildingID = this.building.id;
    this.settingService.getMachineByBuilding(this.buildingID).subscribe((data: any) => {
      this.scalingSetting = data.map(item => item.machineID);
    });
  }
  loadData() {
    const obj: DispatchParams = {
      id: this.value.id,
      glue: this.value.glue,
      lines: this.value.lines,
      estimatedTime: this.value.estimatedTime,
    };
    this.planService.dispatch(obj).subscribe(data => {
      this.data = data;
      this.detechAmoutEmpty();
      this.signalAuto();
    });
  }
  add(data: IDispatch) {
    const obj: IDispatchForCreate = {
      id: 0,
      mixingInfoID: data.mixingInfoID,
      lineID: data.lineID,
      amount: data.real,
      createdTime: new Date(),
      estimatedTime: this.value.estimatedTime,
    };
    this.dispatchService.add(obj).subscribe((res) => {
      this.loadData();
      this.alertify.success('Success');
    }, error => {
        this.alertify.warning('error');
    });
  }
}
