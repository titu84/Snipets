 static int modulo(int a, int b)
        {
            if (b != 0)
                return a - b * (int)(Math.Floor((decimal)(a / b)));
            throw new ArgumentException("b nie może być zerem!");
        }
