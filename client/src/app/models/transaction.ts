export interface Transaction {
  id: number;
  sourceSystem: string;
  transactionId: number;
  cardNumber: string;
  locationCode: string;
  productName: string;
  amount: number;
  transactionTime: string;
  status: 'Active' | 'Revoked';
}
