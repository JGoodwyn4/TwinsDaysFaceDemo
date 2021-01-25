using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwinsDay2017_Demo
{
    class Settings
    {
        #region Variables

        private double FAR;
        private double lastFAR;
        private List<string> databases;
        private List<string> lastDatabases;
        private int numMatches;
        private int lastMatch;
        private int threshold;
        private int lastThreshold;

        #endregion

        #region Constructors

        public Settings() : this(0.01, new List<string>(), 0, 24) {}

        public Settings(Settings otherSettings) : this(otherSettings.FAR, otherSettings.databases, otherSettings.numMatches, otherSettings.threshold) {}

        public Settings(double match_FAR, List<string> databases, int numMatches, int threshold)
        {
            this.FAR = match_FAR;
            this.lastFAR = match_FAR;
            this.databases = databases;
            this.lastDatabases = databases;
            this.numMatches = numMatches;
            this.lastMatch = numMatches;
            this.threshold = threshold;
            this.lastThreshold = threshold;
        }

        #endregion

        #region Actions

        public void AddDatabase(String folderPath)
        {
            this.databases.Add(folderPath);
        }

        public void ChangeDatabase(string oldPath, string newPath)
        {
            bool found = false;
            for (int i = 0; !found && i < databases.Count; i++)
            {
                if (databases[i].Equals(oldPath, StringComparison.CurrentCultureIgnoreCase))
                {
                    databases[i] = newPath;
                    found = true;
                }
            }
        }

        public void ClearDatabases()
        {
            this.databases.Clear();
        }

        public void Revert()
        {
            SetNumMatches(this.lastMatch);
            SetDatabases(this.lastDatabases);
            SetFar(this.lastFAR);
            SetThreshold(this.lastThreshold);
        }

        #endregion

        #region Sets/Gets

        public void SetSettings(double match_FAR, List<string> newDatabases, int numMatches, int threshold)
        {
            SetFar(match_FAR);
            SetDatabases(newDatabases);
            SetNumMatches(numMatches);
            SetThreshold(threshold);
        }

        public void SetSettings(Settings otherSettings)
        {
            this.SetSettings(otherSettings.FAR, otherSettings.databases, otherSettings.numMatches, otherSettings.threshold);
        }

        public int GetNumMatches()
        {
            return this.numMatches;
        }

        public void SetNumMatches(int matches)
        {
            this.lastMatch = this.numMatches;
            this.numMatches = matches;
        }

        public List<string> GetDatabases()
        {
            return databases;
        }

        public void SetDatabases(List<string> a)
        {
            this.lastDatabases = new List<string>(this.databases);
            this.databases = a;
        }

        public double GetFar()
        {
            return this.FAR;
        }

        public void SetFar(double far)
        {
            this.lastFAR = this.FAR;
            this.FAR = far;
        }

        public int GetThreshold()
        {
            return this.threshold;
        }

        public void SetThreshold(int th)
        {
            this.lastThreshold = this.threshold;
            this.threshold = th;
        }

        #endregion
    }
}
