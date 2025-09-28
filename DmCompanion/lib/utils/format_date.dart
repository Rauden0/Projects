String formatDate(int year, int month, int day) =>
    '${year > 1 ? 'Year $year, ' : ''}${month > 1 ? 'Month $month, ' : ''}Day $day';
