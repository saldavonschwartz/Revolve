using UnityEngine;
using System;

namespace FEDE {
  namespace Utils {
  
    public static class Math {
      public static readonly float AirDensity = 1.29f;

      public static float Integral(Func<float, float> integrand, float lowerBound, float upperBound, uint samples) {
        // Simpson's rule:
        float h = (upperBound - lowerBound) / samples;
        double integrandL = integrand(lowerBound);
        double integrandU = integrand(upperBound);
        double result = integrandL + integrandU;
        
        for (uint i = 1; i < samples; i += 2) {
          double eval = integrand(lowerBound + i * h);
          result += 4f * eval;
        }

        for (uint i = 2; i < samples - 1; i += 2) {
          double eval = integrand(lowerBound + i * h);
          result += 2f * eval;
        }
        
        return  (float)(result * h / 3f);
      }

      private static float MinimumDivisorSize = Mathf.Pow(10f, -14f);
      public static float FindRoot(Func<float, float> function, Func<float, float> firstDerivative, float x0, float error, uint maxIterations) {
        // Newton's method:
        float x = x0;
        for (uint i = 0; i < maxIterations; i++) {
          float y = function(x0);
          float yPrime = firstDerivative(x0);

          if (Mathf.Abs(yPrime) < MinimumDivisorSize) {
            break;
          }

          x = x0 - y / yPrime;
          if (Mathf.Abs(x - x0) / Mathf.Abs(x) < error) {
            break;
          }

          x0 = x;
        }

        return x;
      }

      public static int WrapAround(int number, int moduloBase) {
        int modulo = number % moduloBase;
        if (modulo < 0) {
          return moduloBase + modulo;
        }
        else {
          return modulo;
        }
      }
    
      public static float WrapAround(float number, float moduloBase) {
        float modulo = number % moduloBase;
        if (modulo < 0) {
          return moduloBase + modulo;
        }
        else {
          return modulo;
        }
      }
    }
    
  }
}
