using System;
using System.ComponentModel.DataAnnotations;
using System.Numerics;
using System.Security.Cryptography;

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
            if (coabs == 0) continue;
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

    public static Polynomial operator +(Polynomial p1, Polynomial p2)
    {
        int max = Math.Max(p1.Degree, p2.Degree);
        double[] coefflist = new double[max+1];
        for (int i = 0; i <= max; i++)
        {
            coefflist[i] = p1.Coeffs[i] + p2.Coeffs[i];
        }
        return new Polynomial(coefflist);
    }

    public static Polynomial operator *(Polynomial p1, Polynomial p2)
    {
    int resDegree = p1.Degree + p2.Degree;
    double[] resultCoeffs = new double[resDegree+1];
    for (int i = 0; i <= p1.Degree; i++)
    {
        for (int j = 0; j <= p2.Degree; j++)
        {
            resultCoeffs[i+j] += p1.Coeffs[i] * p2.Coeffs[j];
        }
    }
    return new Polynomial(resultCoeffs);
    }
}

class Programm
{
    static void Main(string[] args)
    {
        double[] coeffs = {1.0, 0.0, 2.0};
        Polynomial p = new Polynomial(coeffs); // 1 + 2x^2
        Console.WriteLine(p);

        coeffs = new double[] {3.0, 2.0, 1.0};
        Polynomial p1 = new Polynomial(coeffs);
        coeffs = new double[] {2.0, 3.0, 0.0};
        Polynomial p2 = new Polynomial(coeffs);
        Console.WriteLine(p1);
        Console.WriteLine(p2);

        Polynomial p3 = p1 + p2;
        Console.WriteLine("p1 + p2 = " + p3); // 5 + 5x + x^2
        Polynomial p4 = p1 * p2;
        Console.WriteLine("p1 * p2 = " + p4); // 6 + 13x + 8x^2 + 3x^3
    }
}
