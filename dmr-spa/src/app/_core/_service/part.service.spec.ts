/* tslint:disable:no-unused-variable */

import { TestBed, inject, waitForAsync } from '@angular/core/testing';
import { PartService } from './part.service';

describe('Service: Part', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [PartService]
    });
  });

  it('should ...', inject([PartService], (service: PartService) => {
    expect(service).toBeTruthy();
  }));
});
