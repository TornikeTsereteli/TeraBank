# Loan Management API

## Overview

The **Loan Management API** is designed for clients to manage their loans, including applying for new loans, checking the status of existing loans, viewing loan history, handling penalties, and making payments towards their loans and penalties.


## Features

- **Loan Application**: Clients can apply for loans based on predefined conditions such as eligibility criteria, amount, interest rate, and loan duration.
- **Loan Status**: Clients can check the status of their loan, including remaining amount, monthly payment, and loan lifecycle information.
- **Loan History**: Clients can view a history of their loans and track repayment progress.
- **Penalty Fees**: Clients can check for penalty fees, which are automatically calculated based on late payments or other rule violations.
- **Payments**: Clients can make payments toward their loans and penalties directly via the API.

---

## Business Logic

### 1. **Loan Application Logic**

When a client applies for a loan:

- **Eligibility Check**: The system first verifies whether the client is eligible for a loan based on age or any other criteria (for example, minimum age requirement).
- **Loan Details**: The loan amount, interest rate, and duration in months are provided by the client. These values are used to calculate the monthly payment and repayment schedule.
- **Interest Rate and Duration**: The system will apply an interest rate to the loan amount based on the provided value, and the loan's start and end dates are calculated according to the duration in months.
- **Loan Status**: Initially, the loan status is set to "Pending," indicating that it is awaiting approval. After a successful application, the loan enters a "Pending" state and can be moved to "Approved" after further checks.

#### Example:
A client applies for a loan of **$10,000** with a **5% interest rate** and a duration of **24 months**. The system will calculate the monthly payment using the loan's amount, interest rate, and duration.

---

### 2. **Loan Status Management**

Once a loan is applied, the client can check the status of the loan at any time.

- **Status**: The loan can be in various stages, such as `Pending`, `Approved`, `Rejected`, or `Closed`. Each status reflects the current state of the loan.
- **Remaining Amount**: The remaining balance of the loan is tracked and updated after each payment. Initially, this is equal to the loan amount.
- **Monthly Payment Calculation**: Based on the loan's amount, interest rate, and duration, a monthly payment is calculated. This monthly payment remains constant unless a partial payment or penalty is applied.
 
- სტანდარტული ფორმულა.
---

### 3. **Penalty Fees**

Penalties are automatically applied if a client fails to make payments on time, or if other loan rules are violated. Each penalty is calculated based on the amount owed and the delay.

- **Imposing Penalties**: When a loan payment is overdue, penalties are calculated საწყისი სესხის 1% ყოველდღე

---

### 4. **Payments**

There are two types of payments that a client can make:

- **Loan Payment**: Clients can make payments toward the remaining balance of the loan. Each payment reduces the principal and the remaining amount. The payment also impacts the loan status (moving it closer to being paid off).
- **Penalty Payment**: Clients can also make payments towards their penalties. Once a penalty is paid off, its status is updated to "Paid."

The client can make payments using the following logic:

1. **Loan Payment Process**:
    - When the client submits a payment, the system checks whether the payment amount is sufficient to cover the remaining loan amount.
    - The payment is applied to the remaining loan balance and updates the loan status if the loan is paid off.

2. **Penalty Payment Process**:
    - When a payment is made toward penalties, the system first checks whether the amount covers the outstanding penalty fees.
    - Once the penalty fee is paid off, the penalty is marked as "Paid."

---

მოკლედ რომ ვთქვათ, ე.ი. ჯერ რეგისტრაციაა საჭირო, შემდეგ email დაკონფირმება, შემდეგ login,

სესხის ლოგიკა მდგომარეობს შემდეგში:

1. სესხს აიღებ თუ კრიტერიუმებს აკმაყოფილებ
2. თუ სესხის ყოველთვიურ დავალიანება არ გადაიხდი, გეკისრება ჯარიმა გამოტანილი სესხის 1% ყოველდღე, ამას ხელმზღვანელობს backgroundtask
3. სესხის გადახდისას არხარ ვალდებული ზუსტი თანხა გადაიხადო მთავარია ერთი თვის განმავლობაში დაფარო.
4. გაქვს უფლება overpayment გააკეთო, მაგ შემთხვევაში დგება სესხის გადახდის ახალი გეგმა შემცირებული რიცხვებით.
5. კლიენტს აქვს შესაძლებლობა გადახდები ნახოს, ასევე აქვს შესაძლებლობა სესხის სტატუსი შეამოწმომს რაც უჩვენებს რამდენი უკვე თვიური გადასახადი და ამ თვეს რამდენი დარჩა გადახდა.
6. ჯარიმების გადახდის ლოგიკა ცალკეცაა გადატინილი შენ კლიენტს შეგიძლია როგორც პირდაპირ გადაიხადო მარტო ჯარიმა, ან შეგიძლია გადაიხადო makepayment
7. კლიენტს ასევე შეუძლია ნახოს ყველა გამოტანილი სესხი.


ტესტები  მთავარ ბიზნესს ლოგიკაზე დაწერილია, ასევე ასე თუ ისე დოკუმენტიაციაც არის და კომენტარებიც, + ლოგერები.


P.S.  კონტრიბუტერებში  ჩემ გარდა წერია nikachiradze, ნაწილი კომიტები PCდან გავაკეთე, ადრე მისი კომპიტერი იყო, ლოკალზე ისევ მისი მონაცემებია და მაგიტომაც არის რაღაც კომიტები მის სახელზე  
