using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioVisualizer : MonoBehaviour {

	[SerializeField] int barCount;
    [SerializeField] float minLength;
    [SerializeField] float maxLength;
    [SerializeField] float multiplier;
    [SerializeField] AudioBar barPrefab;
    AudioBar[] bars;

    void Awake() { 
		InitializeBars();
        RadialLayout layout = GetComponent<RadialLayout>();
        if (layout != null) {
            layout.MaxAngle -= (layout.MaxAngle-layout.MinAngle)/barCount; 
        }
    }

	void InitializeBars(bool quiz = false) {
		if (bars != null)
			return;
		bars = new AudioBar[barCount];
		for (int i = 0; i < barCount; ++i) {
			bars[i] = Instantiate(barPrefab.gameObject, transform).GetComponent<AudioBar>();
			bars[i].SetQuiz(quiz);
		}
	}

	void OnDisable() {
		for (int i = 0; i < barCount; ++i) {
			bars[i].SetLength(0);
		}
	}

	public void SetQuiz(bool on) {
		if (bars == null)
			InitializeBars(on);
		else {
			for (int i = 0; i < barCount; ++i) {
				bars[i].SetQuiz(on);
			}
		}
	}

	public void Visualize(float[] samples) {
        Complex[] spec = new Complex[samples.Length];
        for (int i = 0; i < samples.Length; ++i) {
            spec[i] = new Complex(samples[i], 0);
        }
        MathUtility.CalculateFFT(spec,false);
        //BaseVisualization(spec);
       MaxVisualization(spec);
    }

    public void BaseVisualization(Complex[] spec) {
        int count = spec.Length/2;
        int offset = 0;
        int sampleCount = 0;
        double average = 0, current = 0;
        float pow = Mathf.Log(count,2);
        for (int i = 0; i < bars.Length; ++i) {
            sampleCount = Mathf.Clamp(Mathf.RoundToInt(Mathf.Pow(2, pow * (i+1.0f) / bars.Length)), 1, count) - offset;

            for (int j = offset; j < offset + sampleCount; j++) {
                current = spec[j].magnitude * 2;
                average += (float) current;
                if (j == count - 1) {
                    sampleCount = j - offset;
                    break;
                }
            }
            offset += sampleCount;
            average /= sampleCount;
            bars[i].Stretch((float)(average * multiplier), minLength, maxLength);
            if (offset == count - 1)
                break;
                
            //bars[i].Stretch((float)(spec[i].magnitude * 2 * multiplier), minLength, maxLength);
        }
    }

    public void MaxVisualization(Complex[] spec) {
        int count = spec.Length/2;
        int offset = 0;
        int sampleCount = 0;
        double max = 0, current = 0;
        float pow = Mathf.Log(count,2);
        for (int i = 0; i < bars.Length; ++i) {
            sampleCount = Mathf.Clamp(Mathf.RoundToInt(Mathf.Pow(2, pow * (i+1.0f) / bars.Length)), 1, count) - offset;
            max = 0;

            for (int j = offset; j < offset + sampleCount; j++) {
                current = spec[j].magnitude * 2;
                if (current > max)
                    max = current;
                if (j == count - 1) {
                    sampleCount = j - offset;
                    break;
                }
            }
            offset += sampleCount;
            bars[i].Stretch((float)(max * multiplier), minLength, maxLength);
            if (offset == count - 1)
                break;
                
            //bars[i].Stretch((float)(spec[i].magnitude * 2 * multiplier), minLength, maxLength);
        }
    }

    public void AverageVisualization(Complex[] spec) {
        int count = spec.Length/2;
        int offset = 0;
        int sampleCount = 0;
        double average = 0, current = 0;
        for (int i = 0; i < bars.Length; ++i) {
            sampleCount = (int)(count / (float)bars.Length);

            for (int j = offset; j < offset + sampleCount; j++) {
                current = spec[j].magnitude * 4;
                average += (float) current;
                if (j == count - 1) {
                    sampleCount = j - offset;
                    break;
                }
            }
            offset += sampleCount;
            average /= sampleCount;
            bars[i].Stretch((float)(average * multiplier), minLength, maxLength);
            if (offset == count - 1)
                break;
                
            //bars[i].Stretch((float)(spec[i].magnitude * 2 * multiplier), minLength, maxLength);
        }
    }

}
