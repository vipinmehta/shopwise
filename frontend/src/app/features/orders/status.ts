export function statusClass(status: string): string {
  switch (status) {
    case 'Delivered': return 'ok';
    case 'Cancelled': return 'bad';
    case 'Returned':
    case 'ReturnRequested': return 'warn';
    default: return '';
  }
}

export const ORDER_STATUSES = ['Placed', 'Confirmed', 'Shipped', 'Delivered', 'Cancelled', 'ReturnRequested', 'Returned'];
