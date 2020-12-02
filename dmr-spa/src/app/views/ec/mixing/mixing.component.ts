import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { HubConnectionState } from '@microsoft/signalr';
import { IBuilding } from 'src/app/_core/_model/building';
import { IMixingInfo } from 'src/app/_core/_model/plan.js';
import { IRole } from 'src/app/_core/_model/role.js';
import { IIngredient } from 'src/app/_core/_model/summary';
import { AbnormalService } from 'src/app/_core/_service/abnormal.service';
import { AlertifyService } from 'src/app/_core/_service/alertify.service';
import { IngredientService } from 'src/app/_core/_service/ingredient.service';
import { MakeGlueService } from 'src/app/_core/_service/make-glue.service';
import { PlanService } from 'src/app/_core/_service/plan.service.js';
import { SettingService } from 'src/app/_core/_service/setting.service.js';
import * as signalr from '../../../../assets/js/ec-client.js';
const SUMMARY_RECIEVE_SIGNALR = 'ok';
const UNIT_BIG_MACHINE = 'k';
const UNIT_SMALL_MACHINE = 'g';
const BUILDING_LEVEL = 2;
declare var $: any;
const ADMIN = 1;
const SUPERVISOR = 2;


@Component({
  selector: 'app-mixing',
  templateUrl: './mixing.component.html',
  styleUrls: ['./mixing.component.css']
})
export class MixingComponent implements OnInit {
  ingredients: IIngredient[];
  ingredientsTamp: IIngredient[];
  building: IBuilding;
  glueID: number;
  disabled = true;
  unit = 'k';
  position: string;
  qrCode: string;
  buildingName: string;
  IsAdmin: true;
  scalingSetting: any;
  buildingID: number;
  scalingKG: string;
  volume: number;
  volumeA: number;
  volumeB: any;
  volumeC: any;
  volumeD: any;
  volumeE: any;
  volumeH: any;
  B: number;
  C: number;
  D: number;
  endTime: Date;
  guidances: IMixingInfo;
  makeGlue: any;
  startTime: any;
  glueName: string;
  role: IRole;
  estimatedTime: any;
  constructor(
    private route: ActivatedRoute,
    private alertify: AlertifyService,
    private ingredientService: IngredientService,
    private makeGlueService: MakeGlueService,
    private abnormalService: AbnormalService,
    private planService: PlanService,
    private router: Router,
    private settingService: SettingService
  ) { }

  ngOnInit() {
    const BUIDLING: IBuilding = JSON.parse(localStorage.getItem('building'));
    const ROLE: IRole = JSON.parse(localStorage.getItem('level'));
    this.role = ROLE;
    this.building = BUIDLING;
    this.scalingKG = UNIT_BIG_MACHINE;
    this.startTime = new Date();
    this.getScalingSetting();
    this.onRouteChange();
  }
  onRouteChange() {
    this.route.data.subscribe(data => {
      this.glueID = this.route.snapshot.params.glueID;
      this.estimatedTime = this.route.snapshot.params.estimatedTime;
      this.getGlueWithIngredientByGlueID();
    });
  }
  getGlueWithIngredientByGlueID() {
    this.makeGlueService
      .getGlueWithIngredientByGlueID(this.glueID)
      .subscribe((res: any) => {
        this.ingredients = res.ingredients.map((item) => {
          return {
            id: item.id,
            scanStatus: item.position === 'A',
            code: item.code,
            scanCode: '',
            materialNO: item.materialNO,
            name: item.name,
            percentage: item.percentage,
            position: item.position,
            allow: item.allow,
            expected: 0,
            real: 0,
            focusReal: false,
            focusExpected: false,
            valid: false,
            info: '',
            batch: '',
            unit: ''
          };
        });
        this.glueName = res.name;
      });
  }

  // khi scan qr-code
  async onNgModelChangeScanQRCode(args, item) {
    this.ingredientsTamp = item;
    this.position = item.position;
    const input = args.split('-') || [];
    if (input[2]?.length === 8) {
      try {
        this.qrCode = input[2];
        const result = await this.scanQRCode();
        if (this.qrCode !== item.materialNO) {
          this.alertify.warning(`Please you should look for the chemical name "${item.name}"`);
          this.qrCode = '';
          this.errorScan();
          return;
        }
        // const checkIncoming = await this.checkIncoming(item.name, this.level.name, input[1]);
        // if (checkIncoming === false) {
        //   this.alertify.error(`Invalid!`);
        //   this.qrCode = '';
        //   this.errorScan();
        //   return;
        // }

        const checkLock = await this.hasLock(
          item.name,
          this.building.name,
          input[1]
        );
        if (checkLock === true) {
          this.alertify.error('This chemical has been locked!');
          this.qrCode = '';
          this.errorScan();
          return;
        }

        /// Khi quét qr-code thì chạy signal
        this.signal();

        const code = result.code;
        const ingredient = this.findIngredientCode(code);
        this.setBatch(ingredient, input[1]);
        if (ingredient) {
          this.changeInfo('success-scan', ingredient.code);
          if (ingredient.expected === 0 && ingredient.position === 'A') {
            this.changeFocusStatus(ingredient.code, false, true);
            this.changeScanStatus(ingredient.code, false);
          } else {
            this.changeScanStatus(ingredient.code, false);
            this.changeFocusStatus(code, false, false);
          }
        }
        // chuyển vị trí quét khi scan
        switch (this.position) {
          case 'B':
            this.changeScanStatusByPosition('C', true);
            break;
          case 'C':
            this.changeScanStatusByPosition('C', false);
            this.changeScanStatusByPosition('D', true);
            break;
          case 'D':
            this.changeScanStatusByPosition('E', true);
            break;
          case 'E':
            this.changeScanStatusByPosition('H', true);
            break;
        }
      } catch (error) {
        console.log('tag', error);
        this.errorScan();
        this.alertify.error('Wrong Chemical!');
        this.qrCode = '';
      }
    }
  }
  // api
  scanQRCode(): Promise<any> {
    return this.ingredientService.scanQRCode(this.qrCode).toPromise();
  }
  // helpers
  private findIngredientCode(code) {
    for (const item of this.ingredients) {
      if (item.code === code) {
        return item;
      }
    }
  }
  private setBatch(item, batch) {
    for (const i in this.ingredients) {
      if (this.ingredients[i].id === item.id) {
        this.ingredients[i].batch = batch;
        break;
      }
    }
  }
  private changeScanStatus(code, scanStatus) {
    for (const i in this.ingredients) {
      if (this.ingredients[i].code === code) {
        this.ingredients[i].scanStatus = scanStatus;
        break; // Stop this loop, we found it!
      }
    }
  }
  private changeFocusStatus(code, focusReal, focusExpected) {
    for (const i in this.ingredients) {
      if (this.ingredients[i].code === code) {
        this.ingredients[i].focusReal = focusReal;
        this.ingredients[i].focusExpected = focusExpected;
        break; // Stop this loop, we found it!
      }
    }
  }
  private changeValidStatus(code, validStatus) {
    for (const i in this.ingredients) {
      if (this.ingredients[i].code === code) {
        this.ingredients[i].valid = validStatus;
        break; // Stop this loop, we found it!
      }
    }
  }
  private changeScanStatusByPosition(position, scanStatus) {
    this.position = position;
    for (const i in this.ingredients) {
      if (this.ingredients[i].position === position) {
        this.ingredients[i].scanStatus = scanStatus;
        break;
        // Stop this loop, we found it!
      }
    }
  }
  private errorScan() {
    for (const key in this.ingredients) {
      if (this.ingredients[key].scanStatus) {
        const element = this.ingredients[key];
        this.changeInfo('error-scan', element.code);
      }
    }
  }
  private changeInfo(info, code) {
    for (const i in this.ingredients) {
      if (this.ingredients[i].code === code) {
        this.ingredients[i].info = info;
        break; // Stop this loop, we found it!
      }
    }
  }
  private changeScanStatusFocus(position, status) {
    for (const i in this.ingredients) {
      if (this.ingredients[i].position === position) {
        this.ingredients[i].scanStatus = status;
        break; // Stop this loop, we found it!
      }
    }
  }
  private findIngredient(position) {
    for (const item of this.ingredients) {
      if (item.position === position) {
        return item;
      }
    }
  }
  private calculatorIngredient(weight, percentage) {
    const result = (weight * percentage) / 100;
    return result * 1000 ?? 0;
  }
  private toFixedIfNecessary(value, dp) {
    return +parseFloat(value).toFixed(dp);
  }
  private changeExpectedRange(args, position) {
    const positionArray = ['A', 'B', 'C', 'D', 'E'];
    if (positionArray.includes(position)) {
      const weight = parseFloat(args);
      const expected = this.calculatorIngredient(
        weight,
        this.findIngredient(position)?.percentage
      );
      if (position === 'B') {
        this.B = expected;
      }
      if (position === 'C') {
        this.C = expected;
      }
      if (position === 'D') {
        this.D = expected;
      }
      const allow = this.calculatorIngredient(
        expected / 1000,
        this.findIngredient(position)?.allow
      );
      const min = expected - allow;
      const max = expected + allow;
      const minRange = this.toFixedIfNecessary(min / 1000, 3);
      const maxRange = this.toFixedIfNecessary(max / 1000, 3);
      const expectedRange =
        maxRange > 3
          ? `${minRange}kg - ${maxRange}kg`
          : ` ${this.toFixedIfNecessary(min, 1)}g - ${this.toFixedIfNecessary(max, 1)}g `;
      if (allow === 0) {
        const kgValue = this.toFixedIfNecessary(expected / 1000, 3);
        // tslint:disable-next-line:no-shadowed-variable
        const expectedRange = kgValue > 3 ? `${kgValue}kg` : ` ${this.toFixedIfNecessary(kgValue * 1000, 1)}g`;
        this.changeExpected(position, expectedRange);
      } else {
        this.changeExpected(position, expectedRange);
      }
    }
  }
  private changeExpected(position, expected) {
    for (const i in this.ingredients) {
      if (this.ingredients[i].position === position) {
        const expectedResult = expected;
        // const expectedResult = this.toFixedIfNecessary(expected, 2);
        this.ingredients[i].expected = expectedResult;
        break; // Stop this loop, we found it!
      }
    }
  }

  private changeActualByPosition(position, actual, unit) {
    for (const i in this.ingredients) {
      if (this.ingredients[i].position === position) {
        this.ingredients[i].real = actual;
        this.ingredients[i].unit = position === 'A' ? UNIT_BIG_MACHINE : unit;
        break; // Stop this loop, we found it!
      }
    }
  }
  checkValidPosition(ingredient, args) {
    let min;
    let max;
    let minG;
    let maxG;
    const currentValue = parseFloat(args);

    if (ingredient.allow === 0) {
      let unit = ingredient.expected.replace(/[0-9|.]+/g, '').trim();
      unit = ingredient.position === 'A' ? 'k' : unit;
      if (unit === UNIT_BIG_MACHINE) {
        min = parseFloat(ingredient.expected);
        max = parseFloat(ingredient.expected);
      } else {
        minG = parseFloat(ingredient.expected);
        maxG = parseFloat(ingredient.expected);
        min = parseFloat(ingredient.expected) / 1000;
        max = parseFloat(ingredient.expected) / 1000;
      }
    } else {
      const exp2 = ingredient.expected.split('-');
      const unit = exp2[0].replace(/[0-9|.]+/g, '').trim();
      if (unit === UNIT_BIG_MACHINE) {
        min = parseFloat(exp2[0]);
        max = parseFloat(exp2[1]);
      } else {
        minG = parseFloat(exp2[0]);
        maxG = parseFloat(exp2[1]);
        min = parseFloat(exp2[0]) / 1000;
        max = parseFloat(exp2[1]) / 1000;
      }
    }

    // Nếu Chemical là A, focus vào chemical B
    if (ingredient.position === 'A') {
      const positionArray = ['B', 'C', 'D', 'E'];
      for (const position of positionArray) {
        this.changeExpectedRange(args, position);
      }
      this.changeScanStatusFocus('A', false);
      this.changeScanStatusFocus('B', true);
      this.changeFocusStatus(ingredient.code, false, false);
      if (this.ingredients.length === 1) {
        this.disabled = false;
      }
    }

    // Nếu Chemical là B, focus vào chemical C
    if (ingredient.position === 'B') {
      if (max > 3) {
        this.scalingKG = UNIT_BIG_MACHINE;
        if (currentValue <= max && currentValue >= min) {
          this.changeScanStatusFocus('B', false);
          this.changeScanStatusFocus('C', true);
          this.changeValidStatus(ingredient.code, false);
          this.changeFocusStatus(ingredient.code, false, false);
          if (this.ingredients.length === 2) {
            this.disabled = false;
          }
        } else {
          this.disabled = true;
          this.changeFocusStatus(ingredient.code, false, false);
          this.changeValidStatus(ingredient.code, true);
          // this.alertify.warning(`Invalid!`, true);
        }
      } else {
        this.scalingKG = UNIT_SMALL_MACHINE;
        if (currentValue <= maxG && currentValue >= minG) {
          this.changeScanStatusFocus('B', false);
          this.changeScanStatusFocus('C', true);
          this.changeValidStatus(ingredient.code, false);
          this.changeFocusStatus(ingredient.code, false, false);
          if (this.ingredients.length === 2) {
            this.disabled = false;
          }
        } else {
          this.disabled = true;
          this.changeFocusStatus(ingredient.code, false, false);
          this.changeValidStatus(ingredient.code, true);
          // this.alertify.warning(`Invalid!`, true);
        }
      }
    }

    // Nếu Chemical là C, focus vào chemical D
    if (ingredient.position === 'C') {
      if (max > 3) {
        this.scalingKG = UNIT_BIG_MACHINE;
        if (currentValue <= max && currentValue >= min) {
          this.changeScanStatusFocus('C', false);
          this.changeScanStatusFocus('D', true);
          this.changeValidStatus(ingredient.code, false);
          this.changeFocusStatus(ingredient.code, false, false);
          if (this.ingredients.length === 3) {
            this.disabled = false;
          }
        } else {
          this.disabled = true;
          this.changeFocusStatus(ingredient.code, false, false);
          this.changeValidStatus(ingredient.code, true);
          // this.alertify.warning(`Invalid!`, true);
        }
      } else {
        this.scalingKG = UNIT_SMALL_MACHINE;
        if (currentValue <= maxG && currentValue >= minG) {
          this.changeScanStatusFocus('C', false);
          this.changeScanStatusFocus('D', true);
          this.changeValidStatus(ingredient.code, false);
          this.changeFocusStatus(ingredient.code, false, false);
          if (this.ingredients.length === 3) {
            this.disabled = false;
          }
        } else {
          this.disabled = true;
          this.changeFocusStatus(ingredient.code, false, false);
          this.changeValidStatus(ingredient.code, true);
          // this.alertify.warning(`Invalid!`, true);
        }
      }
    }

    // Nếu Chemical là D, focus vào chemical E
    if (ingredient.position === 'D') {
      if (max > 3) {
        this.scalingKG = UNIT_BIG_MACHINE;
        if (currentValue <= max && currentValue >= min) {
          this.changeScanStatusFocus('D', false);
          this.changeScanStatusFocus('E', true);
          this.changeValidStatus(ingredient.code, false);
          this.changeFocusStatus(ingredient.code, false, false);
          if (this.ingredients.length >= 4) {
            this.disabled = false;
          }
        } else {
          this.disabled = true;
          this.changeFocusStatus(ingredient.code, false, false);
          this.changeValidStatus(ingredient.code, true);
          // this.alertify.warning(`Invalid!`, true);
        }
      } else {
        this.scalingKG = UNIT_SMALL_MACHINE;
        if (currentValue <= maxG && currentValue >= minG) {
          this.changeScanStatusFocus('D', false);
          this.changeScanStatusFocus('E', true);
          this.changeValidStatus(ingredient.code, false);
          this.changeFocusStatus(ingredient.code, false, false);
          if (this.ingredients.length >= 4) {
            this.disabled = false;
          }
        } else {
          this.disabled = true;
          this.changeFocusStatus(ingredient.code, false, false);
          this.changeValidStatus(ingredient.code, true);
          // this.alertify.warning(`Invalid!`, true);
        }
      }
    }

    if (ingredient.position === 'E') {
      if (max > 3) {
        this.scalingKG = UNIT_BIG_MACHINE;
        if (currentValue <= max && currentValue >= min) {
          this.changeScanStatusFocus('D', false);
          this.changeScanStatusFocus('E', true);
          this.changeValidStatus(ingredient.code, false);
          this.changeFocusStatus(ingredient.code, false, false);
          if (this.ingredients.length >= 4) {
            this.disabled = false;
          }
        } else {
          this.disabled = true;
          this.changeFocusStatus(ingredient.code, false, false);
          this.changeValidStatus(ingredient.code, true);
          // this.alertify.warning(`Invalid!`, true);
        }
      } else {
        this.scalingKG = UNIT_SMALL_MACHINE;
        if (currentValue <= maxG && currentValue >= minG) {
          this.changeScanStatusFocus('D', false);
          this.changeScanStatusFocus('E', true);
          this.changeValidStatus(ingredient.code, false);
          this.changeFocusStatus(ingredient.code, false, false);
          if (this.ingredients.length >= 4) {
            this.disabled = false;
          }
        } else {
          this.disabled = true;
          this.changeFocusStatus(ingredient.code, false, false);
          this.changeValidStatus(ingredient.code, true);
          // this.alertify.warning(`Invalid!`, true);
        }
      }
    }
    this.changeReal(ingredient.code, args);
  }
  private offSignalr() {
    signalr.SCALING_CONNECTION_HUB.off('Welcom');
  }
  private onSignalr() {
    signalr.SCALING_CONNECTION_HUB.on('Welcom');
  }
  private changeScanStatusByLength(length, item) {
    switch (length) {
      case 2:
        this.offSignalr();
        break;
      case 3:
        if (item.position === 'B') {
          this.changeScanStatusByPosition('B', false);
          this.changeScanStatusByPosition('C', true);
          this.offSignalr();
        } else {
          this.changeScanStatusByPosition('B', false);
          this.changeScanStatusByPosition('C', false);
          this.offSignalr();
        }
        break; // Focus C
      case 4:
        if (item.position === 'B') {
          this.changeScanStatusByPosition('B', false);
          this.changeScanStatusByPosition('C', true);
          this.offSignalr();
        } else if (item.position === 'C') {
          this.changeScanStatusByPosition('C', false);
          this.changeScanStatusByPosition('D', true);
          this.offSignalr();
        } else {
          this.changeScanStatusByPosition('C', false);
          this.changeScanStatusByPosition('D', false);
          this.offSignalr();
        }
        break; // Focus D
      case 5:
        if (item.position === 'B') {
          this.changeScanStatusByPosition('B', false);
          this.changeScanStatusByPosition('C', true);
          this.offSignalr();
        } else if (item.position === 'C') {
          this.changeScanStatusByPosition('C', false);
          this.changeScanStatusByPosition('D', true);
          this.offSignalr();
        } else if (item.position === 'D') {
          this.changeScanStatusByPosition('D', false);
          this.changeScanStatusByPosition('E', true);
          this.offSignalr();
        } else {
          this.changeScanStatusByPosition('D', false);
          this.changeScanStatusByPosition('E', false);
          this.offSignalr();
        }
        break; // Focus E
    }
  }
  private setActualByExpectedRange(i) {
    const ingredient = this.ingredients[i];
    if (ingredient.allow > 0) {
      const expectedRange = this.ingredients[i].expected.split('-');
      const min = parseFloat(expectedRange[0]);
      const max = parseFloat(expectedRange[1]);
      const actual = this.ingredients[i].real;
      if (actual >= min && actual <= max) {
        const length = this.ingredients.length ?? 0;
        this.changeScanStatusByLength(length, ingredient);
      }
    } else {
      const expected = this.ingredients[i].expected;
      const actual = this.ingredients[i].real;
      if (actual === +expected) {
        const length = this.ingredients.length ?? 0;
        this.changeScanStatusByLength(length, ingredient);
      }
    }
  }
  private changeReal(code, real) {
    for (const i in this.ingredients) {
      if (this.ingredients[i].code === code) {
        if (this.ingredients[i].position !== 'A') {
          this.setActualByExpectedRange(i);
        }
        this.ingredients[i].real = this.toFixedIfNecessary(real, 3);
        break; // Stop this loop, we found it!
      }
    }
  }
  private findIngredientRealByPosition(position): number {
    let real = 0;
    for (const item of this.ingredients) {
      if (item.position === position) {
        if (item.unit === UNIT_BIG_MACHINE) {
          real = item.real;
        } else {
          real = (item.real) / 1000;
        }
        break;
      }
    }
    return real;
  }
  private findIngredientBatchByPosition(position) {
    let batch = '';
    for (const item of this.ingredients) {
      if (item.position === position) {
        batch = item.batch;
        break;
      }
    }
    return batch;
  }
  private startScalingHub() {
    signalr.SCALING_CONNECTION_HUB.start().then(() => {
      signalr.SCALING_CONNECTION_HUB.on('Scaling Hub UserConnected', (conId) => {
        console.log('Scaling Hub UserConnected', conId);
        this.signal();
      });
      signalr.SCALING_CONNECTION_HUB.on('Scaling Hub User Disconnected', (conId) => {
        console.log('Scaling Hub User Disconnected', conId);
      });
      console.log('Scaling Hub Signalr connected');
    }).catch((err) => {
      setTimeout(() => this.startScalingHub(), 5000);
    });
  }
  private signal() {
    if (signalr.SCALING_CONNECTION_HUB.state === HubConnectionState.Connected) {
      signalr.SCALING_CONNECTION_HUB.on(
        'Welcom',
        (scalingMachineID, message, unit) => {
          if (this.scalingSetting.includes(+scalingMachineID)) {
            if (unit === this.scalingKG) {
              this.volume = parseFloat(message);
              this.unit = unit;
              // console.log('Unit', unit);
              /// update real A sau do show real B, tinh lai expected
              switch (this.position) {
                case 'A':
                  this.volumeA = this.volume;
                  break;
                case 'B':
                  if (unit === UNIT_BIG_MACHINE) {
                    this.volumeB = this.volume;
                    this.changeActualByPosition('A', this.volumeB, unit);
                    this.checkValidPosition(this.ingredientsTamp, this.volumeB);
                  } else {
                    this.changeActualByPosition('A', this.volumeB, unit);
                    this.checkValidPosition(this.ingredientsTamp, this.volumeB);
                  }
                  break;
                case 'C':
                  this.volumeC = this.volume;
                  this.changeActualByPosition('B', this.volumeC, unit);
                  this.checkValidPosition(this.ingredientsTamp, this.volumeC);
                  break;
                case 'D':
                  this.volumeD = this.volume;
                  this.changeActualByPosition('C', this.volumeD, unit);
                  this.checkValidPosition(this.ingredientsTamp, this.volumeD);
                  break;
                case 'E':
                  this.volumeE = this.volume;
                  this.changeActualByPosition('D', this.volumeE, unit);
                  this.checkValidPosition(this.ingredientsTamp, this.volumeE);
                  break;
                case 'H':
                  this.volumeH = this.volume;
                  this.changeActualByPosition('E', this.volumeH, unit);
                  this.checkValidPosition(this.ingredientsTamp, this.volumeH);
                  break;
              }
            }
          }
        }
      );
    } else {
      this.startScalingHub();
    }
  }
  // event
  showArrow(item): boolean {
    if (item.position === 'A' && item.scanStatus === true) {
      return true;
    }
    if (item.position === 'A' && item.scanStatus === false && item.focusExpected === true) {
      return true;
    }
    if (item.position !== 'A' && item.scanStatus === true) {
      return true;
    }
    return false;
  }
  checkValidPositionForRealEvent(ingredient, args) {
    let min;
    let max;
    let minG;
    let maxG;
    const currentValue = parseFloat(args.target.value);

    if (ingredient.allow === 0) {
      const unit = ingredient.expected.replace(/[0-9|.]+/g, '').trim();
      if (unit === UNIT_BIG_MACHINE) {
        min = parseFloat(ingredient.expected);
        max = parseFloat(ingredient.expected);
        for (const key in this.ingredients) {
          if (this.ingredients[key].id === ingredient.id) {
            this.ingredients[key].valid = currentValue !== max;
            this.ingredients[key].real = currentValue;
            break;
          }
        }
      } else {
        minG = parseFloat(ingredient.expected);
        maxG = parseFloat(ingredient.expected);
        min = parseFloat(ingredient.expected) / 1000;
        max = parseFloat(ingredient.expected) / 1000;
        for (const key in this.ingredients) {
          if (this.ingredients[key].id === ingredient.id) {
            this.ingredients[key].valid = currentValue <= minG || currentValue >= maxG;
            this.ingredients[key].real = currentValue;
            break;
          }
        }
      }
    } else {
      const exp2 = ingredient.expected.split('-');
      const unit = exp2[0].replace(/[0-9|.]+/g, '').trim();
      if (unit === UNIT_BIG_MACHINE) {
        min = parseFloat(exp2[0]);
        max = parseFloat(exp2[1]);
        for (const key in this.ingredients) {
          if (this.ingredients[key].id === ingredient.id) {
            this.ingredients[key].valid = currentValue !== max;
            this.ingredients[key].real = currentValue;
            break;
          }
        }
      } else {
        minG = parseFloat(exp2[0]);
        maxG = parseFloat(exp2[1]);
        min = parseFloat(exp2[0]) / 1000;
        max = parseFloat(exp2[1]) / 1000;
        for (const key in this.ingredients) {
          if (this.ingredients[key].id === ingredient.id) {
            this.ingredients[key].valid = currentValue <= minG || currentValue >= maxG;
            this.ingredients[key].real = currentValue;
            break;
          }
        }
      }
    }
    // Nếu Chemical là A, focus vào chemical B
    if (ingredient.position === 'A') {
      const positionArray = ['B', 'C', 'D', 'E'];
      for (const position of positionArray) {
        this.changeExpectedRange(args, position);
      }
      this.changeScanStatusFocus('A', false);
      this.changeScanStatusFocus('B', true);
      this.changeFocusStatus(ingredient.code, false, false);
      if (this.ingredients.length === 1) {
        this.disabled = false;
      } else {
        this.onSignalr();
      }
    }

    // Nếu Chemical là B, focus vào chemical C
    if (ingredient.position === 'B') {
      if (max > 3) {
        this.scalingKG = UNIT_BIG_MACHINE;
        if (currentValue <= max && currentValue >= min) {
          this.changeScanStatusFocus('B', false);
          this.changeScanStatusFocus('C', true);
          this.changeValidStatus(ingredient.code, false);
          this.changeFocusStatus(ingredient.code, false, false);
          if (this.ingredients.length === 2) {
            this.disabled = false;
          } else {
            this.onSignalr();
          }
        } else {
          this.disabled = true;
          this.changeFocusStatus(ingredient.code, false, false);
          this.changeValidStatus(ingredient.code, true);
          // this.alertify.warning(`Invalid!`, true);
        }
      } else {
        this.scalingKG = UNIT_SMALL_MACHINE;
        if (currentValue <= maxG && currentValue >= minG) {
          this.changeScanStatusFocus('B', false);
          this.changeScanStatusFocus('C', true);
          this.changeValidStatus(ingredient.code, false);
          this.changeFocusStatus(ingredient.code, false, false);
          if (this.ingredients.length === 2) {
            this.disabled = false;
          } else {
            this.onSignalr();
          }
        } else {
          this.disabled = true;
          this.changeFocusStatus(ingredient.code, false, false);
          this.changeValidStatus(ingredient.code, true);
          // this.alertify.warning(`Invalid!`, true);
        }
      }
    }

    // Nếu Chemical là C, focus vào chemical D
    if (ingredient.position === 'C') {
      if (max > 3) {
        this.scalingKG = UNIT_BIG_MACHINE;
        if (currentValue <= max && currentValue >= min) {
          this.changeScanStatusFocus('C', false);
          this.changeScanStatusFocus('D', true);
          this.changeValidStatus(ingredient.code, false);
          this.changeFocusStatus(ingredient.code, false, false);
          if (this.ingredients.length === 3) {
            this.disabled = false;
          } else {
            this.onSignalr();
          }
        } else {
          this.disabled = true;
          this.changeFocusStatus(ingredient.code, false, false);
          this.changeValidStatus(ingredient.code, true);
          // this.alertify.warning(`Invalid!`, true);
        }
      } else {
        this.scalingKG = UNIT_SMALL_MACHINE;
        if (currentValue <= maxG && currentValue >= minG) {
          this.changeScanStatusFocus('C', false);
          this.changeScanStatusFocus('D', true);
          this.changeValidStatus(ingredient.code, false);
          this.changeFocusStatus(ingredient.code, false, false);
          if (this.ingredients.length === 3) {
            this.disabled = false;
          } else {
            this.onSignalr();
          }
        } else {
          this.disabled = true;
          this.changeFocusStatus(ingredient.code, false, false);
          this.changeValidStatus(ingredient.code, true);
          // this.alertify.warning(`Invalid!`, true);
        }
      }
    }

    // Nếu Chemical là D, focus vào chemical E
    if (ingredient.position === 'D') {
      if (max > 3) {
        this.scalingKG = UNIT_BIG_MACHINE;
        if (currentValue <= max && currentValue >= min) {
          this.changeScanStatusFocus('D', false);
          this.changeScanStatusFocus('E', true);
          this.changeValidStatus(ingredient.code, false);
          this.changeFocusStatus(ingredient.code, false, false);
          if (this.ingredients.length >= 4) {
            this.disabled = false;
          } else {
            this.onSignalr();
          }
        } else {
          this.disabled = true;
          this.changeFocusStatus(ingredient.code, false, false);
          this.changeValidStatus(ingredient.code, true);
          // this.alertify.warning(`Invalid!`, true);
        }
      } else {
        this.scalingKG = UNIT_SMALL_MACHINE;
        if (currentValue <= maxG && currentValue >= minG) {
          this.changeScanStatusFocus('D', false);
          this.changeScanStatusFocus('E', true);
          this.changeValidStatus(ingredient.code, false);
          this.changeFocusStatus(ingredient.code, false, false);
          if (this.ingredients.length >= 4) {
            this.disabled = false;
          } else {
            this.onSignalr();
          }
        } else {
          this.disabled = true;
          this.changeFocusStatus(ingredient.code, false, false);
          this.changeValidStatus(ingredient.code, true);
          // this.alertify.warning(`Invalid!`, true);
        }
      }
    }

    if (ingredient.position === 'E') {
      if (max > 3) {
        this.scalingKG = UNIT_BIG_MACHINE;
        if (currentValue <= max && currentValue >= min) {
          this.changeScanStatusFocus('D', false);
          this.changeScanStatusFocus('E', true);
          this.changeValidStatus(ingredient.code, false);
          this.changeFocusStatus(ingredient.code, false, false);
          if (this.ingredients.length >= 4) {
            this.disabled = false;
          } else {
            this.onSignalr();
          }
        } else {
          this.disabled = true;
          this.changeFocusStatus(ingredient.code, false, false);
          this.changeValidStatus(ingredient.code, true);
          // this.alertify.warning(`Invalid!`, true);
        }
      } else {
        this.scalingKG = UNIT_SMALL_MACHINE;
        if (currentValue <= maxG && currentValue >= minG) {
          this.changeScanStatusFocus('D', false);
          this.changeScanStatusFocus('E', true);
          this.changeValidStatus(ingredient.code, false);
          this.changeFocusStatus(ingredient.code, false, false);
          if (this.ingredients.length >= 4) {
            this.disabled = false;
          } else {
            this.onSignalr();
          }
        } else {
          this.disabled = true;
          this.changeFocusStatus(ingredient.code, false, false);
          this.changeValidStatus(ingredient.code, true);
          // this.alertify.warning(`Invalid!`, true);
        }
      }
    }
    // console.log('change real', this.ingredients);
    // this.changeReal(ingredient.code, args);
  }
  realClass(item) {
    const validClass = item.valid === true ? ' warning-focus' : '';
    const className = item.info + validClass;
    return className;
  }
  lockClass(item) {
    return item.scanCode === true ? '' : 'lock';
  }
  onKeyupReal(ingredient, args) {
    if (args.keyCode === 13) {
      this.checkValidPositionForRealEvent(ingredient, args);
      // this.checkValidPosition(item, args);
      // const buildingName = this.building.name;
      // this.UpdateConsumption(item.code, item.batch, item.real);
      // const obj = {
      //   qrCode: ingredient.code,
      //   batch: ingredient.batch,
      //   consump: ingredient.real,
      //   buildingName,
      // };
      // this.UpdateConsumptionWithBuilding(obj);
    }
  }
  onDblClicked(ingredient, args) {
    const item = this.ingredients.filter(x => x.position === ingredient.position)[0];
    if (item.scanCode !== '') {
      this.ingredients.forEach((part, index, theArray) => {
        this.ingredients[index].scanStatus = false;
      });
      ingredient.focusReal = true;
      this.offSignalr();
    } else {
      this.alertify.warning('Hãy quét mã QR trước!!!', true);
      return;
    }
  }
  onKeyupExpected(item, args) {
    if (args.keyCode === 13) {
      if (item.position === 'A') {
        this.changeExpected('A', args.target.value);
        switch (item.position) {
          case 'A':
            this.changeScanStatusByPosition('B', true);
            break;
          case 'B':
            this.changeScanStatusByPosition('C', true);
            break;
          case 'C':
            this.changeScanStatusByPosition('D', true);
            break;
          case 'D':
            this.changeScanStatusByPosition('E', true);
            break;
        }
        this.resetFocusExpectedAndActual();
      }
    }
  }
  resetFocusExpectedAndActual() {
    let i;
    for (i = 0; i < this.ingredients.length; i++) {
      this.ingredients[i].focusReal = false;
      this.ingredients[i].focusExpected = false;
    }
  }

  // api
  back() {
    this.router.navigate([
      `/ec/execution/todolist-2`,
    ]);
  }
  private getScalingSetting() {
    this.buildingID = this.building.id;
    this.settingService.getMachineByBuilding(this.buildingID).subscribe((data: any) => {
      this.scalingSetting = data.map(item => item.machineID);
    });
  }
  hasLock(ingredient, building, batch): Promise<any> {
    let buildingName = building;
    if (this.IsAdmin) {
      buildingName = this.buildingName;
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
  Finish() {
    if (this.IsAdmin) {
      this.alertify.warning(`Only the workers are able to press "Finished" button!<br> Chỉ có công nhân mới được nhấn "Hoàn Thành!"`, true);
      return;
    }
    const date = new Date();
    const buildingID = this.building.id;
    this.endTime = new Date();
    this.guidances = {
      id: 0,
      glueID: this.glueID,
      glueName: this.glueName,
      chemicalA: this.findIngredientRealByPosition('A') + '',
      chemicalB: this.findIngredientRealByPosition('B') + '',
      chemicalC: this.findIngredientRealByPosition('C') + '',
      chemicalD: this.findIngredientRealByPosition('D') + '',
      chemicalE: this.findIngredientRealByPosition('E') + '',
      batchA: this.findIngredientBatchByPosition('A'),
      batchB: this.findIngredientBatchByPosition('B'),
      batchC: this.findIngredientBatchByPosition('C'),
      batchD: this.findIngredientBatchByPosition('D'),
      batchE: this.findIngredientBatchByPosition('E'),
      createdTime: date,
      mixBy: JSON.parse(localStorage.getItem('user')).User.ID,
      buildingID,
      startTime: this.startTime,
      endTime: this.endTime,
      estimatedTime: this.estimatedTime
    };
    this.onSignalr();
    if (this.guidances) {
      this.makeGlueService.Guidance(this.guidances).subscribe((glue: any) => {
        // this.checkValidPosition(item, args);
        // const buildingName = this.building.name;
        // this.UpdateConsumption(item.code, item.batch, item.real);
        // const obj = {
        //   qrCode: ingredient.code,
        //   batch: ingredient.batch,
        //   consump: ingredient.real,
        //   buildingName,
        // };
        // this.UpdateConsumptionWithBuilding(obj);
        this.router.navigate(['/ec/execution/todolist-2']);
        this.alertify.success('The Glue has been finished successfully');
      });
    }
  }
  convertDate(date: Date) {
    const tzoffset = date.getTimezoneOffset() * 60000; // offset in milliseconds
    const localISOTime = (new Date(Date.now() - tzoffset)).toISOString().slice(0, -1);
    return localISOTime;
  }
}
