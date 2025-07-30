window.formatNumberWithCommas = function(num) {
    let cleanNum = num.toString().replace(/[^\d.]/g, '');
    let parts = cleanNum.split('.');
    parts[0] = parts[0].replace(/\B(?=(\d{3})+(?!\d))/g, ',');
    return parts.join('.');
};
