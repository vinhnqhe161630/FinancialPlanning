export class TermViewModel {
  id: string;
  termName: string;
  creatorId: string;
  duration: number;
  startDate: string;
  planDueDate: string;
  reportDueDate: string;
  status: string;
  user: any; // You might want to define a UserViewModel here if needed

  constructor(data: any = {}) {
    this.id = data.id || '';
    this.termName = data.termName || '';
    this.creatorId = data.creatorId || '';
    this.duration = data.duration || 0;
    this.startDate = data.startDate || '';
    this.planDueDate = data.planDueDate || '';  
    this.reportDueDate = data.reportDueDate || '';
    this.status = data.status || 0;
    this.user = data.user || null;
  }
}
