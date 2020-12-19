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
  editSettings = { showDeleteConfirmDialog: false, allowEditing: false, allowAdding: true, allowDeleting: true, mode: 'Normal' };
  setFocus: any;
  title: string;
  data: IDispatch[];
  dataTemp: IDispatch[] = [];
  buildingID: number;
  scalingSetting: any;
  amount: number;
  role: IRole;
  index: number;
  building: IBuilding;
  indexesArray: number[] = [];
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
    public alertify: AlertifyService,
    private ingredientService: IngredientService,
    private abnormalService: AbnormalService,
    private makeGlueService: MakeGlueService

  ) { }

  ngOnInit() {
    this.toolbarOptions = [
        { text: 'Add', tooltipText: 'Add', prefixIcon: 'fa fa-plus', id: 'Add' },
       'Edit', 'Cancel', 'Delete' ];
    this.title = this.value.glueName;
    const BUIDLING: IBuilding = JSON.parse(localStorage.getItem('building'));
    const ROLE: IRole = JSON.parse(localStorage.getItem('level'));
    const USER = JSON.parse(localStorage.getItem('user')).User;
    this.role = ROLE;
    this.user = USER;
    this.building = BUIDLING;
    this.indexesArray = [];
    this.getScalingSetting();
    this.loadData();
    this.startDispatchingTime = new Date().toLocaleString();
    this.isShow = this.value.mixingInfoID === 0 && !this.value.glueName.includes(' + ');
    this.index = 0;
  }
  onChangeLine(args) {
    this.line = args.itemData.value;
    this.dataTemp[this.index].line = this.line;
    const data = this.data.filter(item => item.line === this.line)[0];
    this.dataTemp[this.index].lineID = data.lineID;
    this.dispatchGrid.refresh();
  }
  actionBegin(args) {
    if (args.action === 'edit') {
      console.log('data', this.dataTemp);
    }
  }
  actionComplete($event){}
  toolbarClick(args) {
    switch (args.item.id) {
      case 'Add':
        if (this.mixedConsumption === 0) {
          this.alertify.warning('Hết keo rồi bạn ơi!', true);
          args.cancel = true;
          return;
        }
        const itemData: IDispatch = {
          id: 0,
          lineID: 0,
          line: this.line,
          standardAmount: this.mixedConsumption,
          mixingInfoID: this.value.mixingInfoID,
          mixedConsumption: this.mixedConsumption,
          glue: this.value.glueName,
          real: 0,
          warningStatus: false,
          scanStatus: false,
          isLock: false,
          isNew: true
        };
        const data = this.data.filter(item => item.line === this.line)[0];
        if (data) {
          itemData.lineID = data.lineID;
        }
        this.dataTemp.push(itemData);
        this.dispatchGrid.refresh();
    }
   }
  keytab(args, index, data: IDispatch) {
    const real = +args.target.value;
    const amount = data.standardAmount * 1000;
    const stdAmount = amount - real;
    if (stdAmount < 0) {
      this.dataTemp[index].warningStatus = true;
      this.dispatchGrid.refresh();
      return;
    }
    this.dataTemp[index].real = real;
    this.dataTemp[index].isLock = true;
    this.dataTemp[index].warningStatus = false;
    this.dataTemp[index].scanStatus = false;
    this.dataTemp[index].standardAmount = this.mixedConsumption;
    this.mixedConsumption = stdAmount / 1000;
    const item = this.data.filter(a => a.line === this.line)[0];
    if (item) {
      this.dataTemp[index].lineID = data.lineID;
    }
    // if (this.mixedConsumption > 0) {
    //   const itemData: IDispatch = {
    //     id: 0,
    //     lineID: 0,
    //     line: this.line,
    //     standardAmount: this.mixedConsumption,
    //     mixingInfoID: this.value.mixingInfoID,
    //     mixedConsumption: this.mixedConsumption,
    //     glue: this.value.glueName,
    //     real: 0,
    //     warningStatus: false,
    //     scanStatus: true,
    //     isLock: false,
    //     isNew: true
    //   };
    //   this.dataTemp.splice(index + 1, 0, itemData);
    // }
    this.dispatchGrid.refresh();
    this.offSignalr();
  }
  onFocus(args, index, data: IDispatch) {
    if (data.id > 0) {
      // this.alertify.error('Đã có dữ liệu', true);
      return;
    }
    data.scanStatus = true;
    this.index = index;
    this.onSignalr();
    this.signal(index, data);
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
    if (this.indexesArray.length === 0) { this.todolistService.setValue(true); return; }
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
  signal(index, data) {
    if (signalr.SCALING_CONNECTION_HUB.state === HubConnectionState.Connected) {
      signalr.SCALING_CONNECTION_HUB.on(
        'Welcom',
        (scalingMachineID, message, unit) => {
          if (this.scalingSetting.includes(+scalingMachineID)) {
            if (unit === UNIT_SMALL_MACHINE) {
              this.amount = parseFloat(message);
              console.log(index, scalingMachineID, message, unit);
              this.dataTemp[index].real = this.amount;
              data.real = this.amount;
              // this.dispatchGrid.dataSource = this.dataTemp;
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
  async validateQRCode(input): Promise<{ status: boolean; ingredient: any; }> {
    if (input[2]?.length !== 8) {
      this.alertify.warning('The QR Code is invalid!<br> Mã QR không hợp lệ! Vui lòng thử lại mã khác.', true);
      return {
        status: false,
        ingredient: null
      };
    }
    this.qrCode = input[2];
    const result = await this.scanQRCode();
    if (result.name !== this.value.glueName) {
      this.alertify.warning(`Please you should look for the chemical name "${this.value.glueName}"<br> Vui lòng quét đúng hóa chất "${this.value.glueName}"!`, true);
      this.qrCode = '';
      return {
        status: false,
        ingredient: null
      };
    }
    const checkLock = await this.hasLock(result.name, input[1]);
    if (checkLock === true) {
      this.alertify.error('This chemical has been locked! <br> Hóa chất này đã bị khóa!', true);
      this.qrCode = '';
      return {
        status: false,
        ingredient: null
      };
    }
    return {
        status: true,
        ingredient: result
      };
  }
  async onNgModelChangeScanQRCode(value) {
    const input = value.split('-') || [];
    const valid  = await this.validateQRCode(input);
    if (valid.status === false) { return; }
    const mixing = {
      glueName: this.value.glueName,
      glueID: this.value.glueID,
      buildingID: this.building.id,
      mixBy: this.user.ID,
      estimatedStartTime: this.value.estimatedStartTime,
      estimatedFinishTime: this.value.estimatedFinishTime,
      details: [{
          amount: this.value.standardConsumption,
          ingredientID: valid.ingredient.id,
          batch: input[1],
          mixingInfoID: 0,
          position: 'A'
      }]
    };
    this.makeGlueService.add(mixing).subscribe(() => {
      this.alertify.success('Success!');
      this.isShow = false;
    }, error => this.alertify.error(error));
  }
  scanQRCode(): Promise<any> {
    return this.ingredientService.scanQRCode(this.qrCode).toPromise();
  }
  hasLock(ingredient, batch): Promise<any> {
    let buildingName = this.building.name;
    if (this.role.id === 1 || this.role.id === 2 ) {
      buildingName = 'E';
    }
    return new Promise((resolve, reject) => {
      this.abnormalService.hasLock(ingredient, buildingName, batch).subscribe(
        (res) => {
          resolve(res);
        },
        (err) => {
          reject(false);
        }
      );
    });
  }
  loadData() {
    const obj: DispatchParams = {
      id: this.value.id,
      glue: this.value.glueName,
      lines: this.value.lineNames,
      estimatedTime: this.value.estimatedFinishTime,
      estimatedStartTime: this.value.estimatedStartTime,
      estimatedFinishTime: this.value.estimatedFinishTime,
    };
    this.todolistService.dispatch(obj).subscribe(data => {
      this.mixedConsumption = data[0].mixedConsumption;
      let mixedConsumptionTemp = 0;
      data.forEach(item => {
        mixedConsumptionTemp += item.real / 1000;
      });
      const mixedCon = +mixedConsumptionTemp.toFixed(2);
      if (this.value.glueName.includes(' + ')) {
        this.unitTitle = 'Actual Consumption';
        this.mixedConsumption = mixedCon === this.mixedConsumption ? 0 : +(this.mixedConsumption - mixedCon).toFixed(2);
      } else {
        this.unitTitle = 'Standard Consumption';
        this.mixedConsumption = this.value.standardConsumption;
      }
      this.data = data.map(item => {
        const itemData: IDispatch = {
          id: item.id,
          lineID: item.lineID,
          line: item.line,
          standardAmount: item.standardAmount,
          mixingInfoID: item.mixingInfoID,
          mixedConsumption: item.mixedConsumption,
          glue: item.glue,
          real: item.real,
          warningStatus: false,
          scanStatus: false,
          isLock: false,
          isNew: false
        };
        return itemData;
      });
      this.dataTemp = data.filter(item => item.id > 0);
      this.detechAmoutEmpty();
      this.signalAuto();
    });
  }
  save() {
    if (this.mixedConsumption > 0) {
      this.alertify.warning('Hãy giao hết keo đã pha thì mới được nhấn nút "Hoàn Thành"', true);
      return;
    }
    this.finishDispatchingTime = new Date().toLocaleString();
    const obj: IDispatchForCreate[] = this.dataTemp.map( item => {
      const itemObj: IDispatchForCreate = {
        id: 0,
        mixingInfoID: this.value.mixingInfoID,
        lineID: item.lineID,
        amount: item.real / 1000,
        standardAmount: item.standardAmount,
        createdTime: new Date(),
        estimatedTime: this.value.estimatedFinishTime,
        startDispatchingTime: this.startDispatchingTime,
        finishDispatchingTime: this.finishDispatchingTime
      };
      return itemObj;
    });
    this.dispatchService.addDispatch(obj).subscribe((res) => {
      this.loadData();
      this.activeModal.close();
      this.alertify.success('Success');
    }, error => {
      this.alertify.warning('error');
    });
  }
  add(data: IDispatch) {
    const obj: IDispatchForCreate = {
      id: 0,
      mixingInfoID: data.mixingInfoID,
      lineID: data.lineID,
      amount: data.real,
      createdTime: new Date(),
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
}
