/* tslint:disable:no-unused-variable */

import { TestBed, inject, waitForAsync } from '@angular/core/testing';
import { RoutineService } from './routine.service';

describe('Service: Routine', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [RoutineService]
    });
  });

  it('should ...', inject([RoutineService], (service: RoutineService) => {
    expect(service).toBeTruthy();
  }));
});
