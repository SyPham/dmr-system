/* tslint:disable:no-unused-variable */
import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { DebugElement } from '@angular/core';

import { MixingGlueComponent } from './mixing-glue.component';

describe('MixingGlueComponent', () => {
  let component: MixingGlueComponent;
  let fixture: ComponentFixture<MixingGlueComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ MixingGlueComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(MixingGlueComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
