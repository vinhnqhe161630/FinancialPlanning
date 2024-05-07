import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TermDetailsComponent } from './term-details.component';

describe('TermDetailsComponent', () => {
  let component: TermDetailsComponent;
  let fixture: ComponentFixture<TermDetailsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TermDetailsComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(TermDetailsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
