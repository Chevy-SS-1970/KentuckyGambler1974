using System;

class Polynomial
{
    private int degree;
    private double[] coeffs;

    public Polynomial()
    {
        degree = 0;
        coeffs = new double[1] { 0.0 };
    }

    public Polynomial(double[] new_coeffs)
    {
        degree = new_coeffs.Length - 1;
        coeffs = (double[])new_coeffs.Clone();
    }

    public int Degree
    {
        get { return degree; }
    }

    public double[] Coeffs
    {
        get { return (double[])coeffs.Clone(); }
    }

    public override string ToString()
    {
        if (degree == 0 && coeffs[0] == 0) return "0";

        string result = "";
        bool first = true;
        for (int i = 0; i <= degree; i++)
        {
            double coeff = coeffs[i];
            if (coeff == 0) continue;

            if (first)
            {
                if (coeff < 0) result += "-";
            }
            else
            {
                result += coeff > 0 ? "+" : "-";
            }

            double coabs = Math.Abs(coeff);
            if (i == 0)
            {
                result += coabs.ToString();
            }
            if (i == 1)
            {
                if (coabs != 1) result += coabs.ToString();
                result += "x";
            }
            else
            {
                if (coabs != 1) result += coabs.ToString();
                result += "x^" + i;
            }
            first = false;
        }
        return result;
    }
}

class Programm
{
    static void Main(string[] args)
    {
        double[] coeffs = { 1.0, 0.0, 2.0 };
        Polynomial p = new Polynomial(coeffs); // 1 + 2x^2

        Console.WriteLine(p);
    }
}