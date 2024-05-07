import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ListAnnualReportsComponent } from './list-annual-reports.component';

describe('ListAnnualReportsComponent', () => {
  let component: ListAnnualReportsComponent;
  let fixture: ComponentFixture<ListAnnualReportsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ListAnnualReportsComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(ListAnnualReportsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
