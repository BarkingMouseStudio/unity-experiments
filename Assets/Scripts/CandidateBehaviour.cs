using UnityEngine;
using System.Collections;

public class CandidateBehaviour : MonoBehaviour {

  protected internal bool completed = false;

  const float duration = 10.0f;
  float now = 0.0f;

  const int fitnessLength = 100;
  int fitnessIndex = 0;
  int fitnessCount = 0;
  float[] fitnessHistory = new float[fitnessLength];

  Rigidbody2D handle;
  Rigidbody2D lower;
  Rigidbody2D wheel;

  public float Fitness {
    get {
      if (fitnessCount > fitnessHistory.Length) {
        var averageFitness = fitnessHistory.Aggregate(0.0f,
          (total, next) => total + next,
          (total) => total / fitnessHistory.Length);
				var normalizedFitnessCount = fitnessCount / fitnessLength;
        return 0.1f * normalizedFitnessCount + 0.9f * averageFitness;
      } else {
        return 1.0f; // Worst case, didn't live long enough
      }
    }
  }

	// Use this for initialization
	void Awake() {
    handle = transform.Find("Cart/Handle").GetComponent<Rigidbody2D>();
    lower = transform.Find("Cart/Lower").GetComponent<Rigidbody2D>();
    wheel = transform.Find("Cart/Wheel").GetComponent<Rigidbody2D>();

    SetGenotype(new List<List<double>>(){
      new List<double>(){-0.4058302557095885,0.2521210815757513,-5.332865077070892,7.407665168866515,0.14045477379113436,0.6768293231725693,-58.56127846054733,-9.874404184520245,0.04888633964583278,0.9351401054300368,-63.32889280747622,18.58414063230157,-0.5381791852414608,-0.13699913024902344,-35.47081716824323,-7.911439090967178,-0.4273441433906555,0.8700608261860907,-15.214023552834988,-15.677474085241556,0.7338666860014207,-0.18538392800837755,-11.31864816416055,11.928714960813522,-0.8387203668244183,0.055621285922825336,-46.045068837702274,-16.809250134974718,0.12544145341962576,-0.023462950717657804,-75.86477911099792,-8.279131073504686,-0.3565202667377889,-0.8347263182513416,-7.913040649145842,-18.29885588027537,-0.3223045477643609,0.14865243341773748,-25.453503034077585,12.825178913772106,-0.2907294984906912,-0.25207078736275434,-48.614497133530676,11.053375629708171,0.7288209716789424,0.17539994325488806,-85.8292557997629,-7.11353512480855,0.6950296037830412,0.04847937868908048,-49.142358684912324,-6.192994313314557,-0.16766278725117445,0.4759996719658375,-56.83437210973352,14.149533528834581},
      new List<double>(){-7.403752664104104,-6.964480448514223,17.153176655992866,-4.896272625774145,10.20842527039349,17.36826341599226,16.884225457906723,14.588113892823458,17.04016056843102,17.816350664943457,5.057464921846986,-16.078465450555086,4.717035843059421,-8.877367274835706,-12.825829265639186,-5.0760292913764715,2.8809707332402468,6.104256762191653,-12.926153726875782,-1.135990647599101,-4.828482121229172,14.239508677273989,2.6491028908640146,13.36763747036457,-12.837479868903756,-10.779782868921757},
    });
	}

	void OnSpawned() {
		SetActive(true);
	}

	void OnDespawned() {
    // Clear fitness history
    fitnessHistory = new float[fitnessLength];
    fitnessIndex = 0;
    fitnessCount = 0;

    now = 0.0f;
    completed = false;
	}

	void Complete() {
		completed = true;
		SetActive(false);
	}

	void FixedUpdate() {
    if (duration - now <= 0) { // Elapsed
      Complete();
      return;
    }

    if (wheel.transform.position.y < 0.0) { // Fallen
      Complete();
      return;
    }

		if (handle.transform.position.y < 0.0) { // Destabilized
			Complete();
			return;
		}

    // var thetaUpper = AngleHelper.GetAngle(upper.rotation);
    // var thetaDotUpper = AngleHelper.GetAngle(upper.angularVelocity);
    var thetaLower = AngleHelper.GetAngle(lower.rotation);
    var thetaDotLower = AngleHelper.GetAngle(lower.angularVelocity);
    var x = wheel.transform.localPosition.x;
    var xDot = wheel.velocity.magnitude;

    float fitness = Mathf.Abs(thetaLower) * 1.0f + Mathf.Abs(thetaDotLower) * 1.0f +
								// Mathf.Abs(thetaUpper) * 1.0f + Mathf.Abs(thetaDotUpper) * 1.0f +
                Mathf.Abs(x) * 30.0f + Math.Abs(xDot) * 30.0f;
    var maxFitness = 180.0f * 7.0f;
		var normalizedFitness = fitness / maxFitness;

    fitnessHistory[fitnessIndex] = normalizedFitness;
    fitnessCount++;
    fitnessIndex++;
    fitnessIndex %= fitnessHistory.Length;

    now += Time.fixedDeltaTime;
	}
}
