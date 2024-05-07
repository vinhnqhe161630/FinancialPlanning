import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AnnualReportDetailsComponent } from './annual-report-details.component';

describe('AnnualReportDetailsComponent', () => {
  let component: AnnualReportDetailsComponent;
  let fixture: ComponentFixture<AnnualReportDetailsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AnnualReportDetailsComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(AnnualReportDetailsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
