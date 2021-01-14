import { Component, OnInit, Input } from '@angular/core';
import { BuildingService } from 'src/app/_core/_service/building.service';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { AlertifyService } from 'src/app/_core/_service/alertify.service';
import { IBuilding, ILunchTime, LunchTime } from 'src/app/_core/_model/building';

@Component({
  selector: 'app-building-modal',
  templateUrl: './building-modal.component.html',
  styleUrls: ['./building-modal.component.css']
})
export class BuildingModalComponent implements OnInit {
  @Input() title: string;
  @Input() building: IBuilding;
  lunchTimeData = new LunchTime().data;
  fields: object = { text: 'content', value: 'content' };
  lunchTimeItem: { startTime: Date; endTime: Date; buildingID: number; };
  constructor(
    public activeModal: NgbActiveModal,
    private buildingService: BuildingService,
    private alertify: AlertifyService,
  ) { }

  ngOnInit() {
  }
  onSelectLunchTime(args) {
    this.lunchTimeItem = {
      startTime: args.itemData.startTime,
      endTime: args.itemData.endTime,
      buildingID: 0
    };
  }
  validation() {
    if (this.building.name === '') {
      this.alertify.warning('Please enter building name!', true);
      return false;
    } else if (this.building.hourlyOutput === 0) {
      this.alertify.warning('Please enter hourly output!', true);
      return false;
    }
    else {
      return true;
    }
  }
  createBuilding() {
    if (this.validation()) {
      if (this.building.parentID > 0) {
        this.buildingService.createSubBuilding(this.building).subscribe(res => {
          const statusKey = 'status';
          const buildingKey = 'building';
          const status = res[statusKey] as boolean;
          const building = res[buildingKey];
          if (status) {
            this.lunchTimeItem.buildingID = building.id;
            this.buildingService.addOrUpdateLunchTime(this.lunchTimeItem).subscribe(() => {
              this.alertify.success('The building has been created!!');
              this.activeModal.dismiss();
              this.buildingService.changeMessage(200);
            }, err => {
              this.alertify.error(err);
            });
          } else {
            this.alertify.error('fail!');
          }
        });
      } else {
        this.buildingService.createMainBuilding(this.building).subscribe(res => {
          const statusKey = 'status';
          const buildingKey = 'building';
          const status = res[statusKey] as boolean;
          const building = res[buildingKey];
          if (status) {
            this.lunchTimeItem.buildingID = building.id;
            this.buildingService.addOrUpdateLunchTime(this.lunchTimeItem).subscribe(() => {
              this.alertify.success('The building has been created!!');
              this.activeModal.dismiss();
              this.buildingService.changeMessage(200);
            }, err => {
              this.alertify.error(err);
            });
          } else {
            this.alertify.error('fail!');
          }
        });
      }
    }
  }
}
