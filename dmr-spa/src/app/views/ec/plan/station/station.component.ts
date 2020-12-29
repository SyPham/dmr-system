import { Component, OnInit, Input, ViewChild } from '@angular/core';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { GridComponent } from '@syncfusion/ej2-angular-grids';
import { IPlan } from 'src/app/_core/_model/plan';
import { IStation } from 'src/app/_core/_model/station';
import { AlertifyService } from 'src/app/_core/_service/alertify.service';
import { StationService } from 'src/app/_core/_service/station.service';
@Component({
  selector: 'app-station',
  templateUrl: './station.component.html',
  styleUrls: ['./station.component.css']
})
export class StationComponent implements OnInit {
  @Input() plan: IPlan;
  @ViewChild('grid') grid: GridComponent;
  station: IStation;
  data: IStation[];
  toolbarOptions = ['Edit', 'Cancel', 'Delete', 'Search'];
  filterSettings: { type: string; };
  editSettings = { showDeleteConfirmDialog: false, allowEditing: true, allowAdding: true, allowDeleting: true, mode: 'Normal' };
  constructor(
    public activeModal: NgbActiveModal,
    private alertify: AlertifyService,
    private stationSevice: StationService,
  ) { }

  ngOnInit(): void {
    this.getAllByPlanID();
    this.toolbarOptions = ['Edit', 'Cancel', 'Delete', 'Search'];
  }
  // grid
  actionBegin(args) {
    if (args.action === 'edit' && args.requestType === 'save') {
      const item = args.data as IStation;
      // const data =  this.grid.dataSource as IStation[];
      // data[args.rowIndex].amount = item.amount;
      // this.grid.refresh();
      this.update(item);
    }
    if (args.action === 'add' && args.requestType === 'save') {
    }
    if (args.action === 'delete' && args.requestType === 'save') {
    }
  }
  actionComplete($event) { }
  toolbarClick(args) {
    switch (args.item.id) {
      case 'Edit':
      break;
    }
  }
  // end grid

  // api
  confirm() {
    // const createData = this.data.filter(item => item.id === 0);
    // const updateData = this.data.filter(item => item.id > 0);
    // if (createData.length > 0) {
    //   this.createRange(createData);
    // }
    // if (updateData.length > 0) {
    //   this.updateRange(updateData);
    // }
    this.activeModal.dismiss();
    this.alertify.success('Updated successed!');
  }
  getAllByPlanID() {
    this.stationSevice.getAllByPlanID(this.plan.id || 0).subscribe(data => {
      this.data = data;
    });
  }
  createRange(data: IStation[]) {
    this.stationSevice.createRange(data).subscribe(res => {
      this.alertify.success('Created successed!');
      this.activeModal.dismiss();
      this.stationSevice.setValue(true);
    });
  }
  update(data: IStation) {
    this.stationSevice.update(data).subscribe(res => {
      this.stationSevice.setValue(true);
    });
  }
  updateRange(data: IStation[]) {
    this.stationSevice.updateRange(data).subscribe(res => {
      this.alertify.success('Updated successed!');
      this.activeModal.dismiss();
      this.stationSevice.setValue(true);
    });
  }
  delete() {
    this.stationSevice.delete(this.station.id).subscribe(res => {
      this.alertify.success('delete successed!');
      this.activeModal.dismiss();
      this.stationSevice.setValue(true);
    });
  }
  // end api
}
