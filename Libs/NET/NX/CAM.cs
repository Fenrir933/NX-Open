using NXWrapper.Interfaces;
using NXOpen.CAM;
using System.Collections.Generic;
using System.Linq;

namespace NXWrapper.CAM {
    internal class CAM : ICAM {
        public CAMSetup Setup { get; }
        public NCGroup ProgramRoot { get; }
        public NCGroup MachineRoot { get; }
        public NCGroup GeometryRoot { get; }
        public NCGroup MethodRoot { get; }
        public NCGroupCollection Collection { get; }
        public OperationCollection Operations { get; }

        internal CAM() {
            Setup = NX.WorkPart.CAMSetup;
            ProgramRoot = Setup.GetRoot(CAMSetup.View.ProgramOrder);
            MachineRoot = Setup.GetRoot(CAMSetup.View.MachineTool);
            GeometryRoot = Setup.GetRoot(CAMSetup.View.Geometry);
            MethodRoot = Setup.GetRoot(CAMSetup.View.MachineMethod);
            Collection = Setup.CAMGroupCollection;
            Operations = Setup.CAMOperationCollection;
        }

        private void GetNCGroupMembersRecursive(NCGroup ncGroup, ref IEnumerable<NCGroup> groups) {
            foreach (var member in ncGroup.GetMembers())
                if (member is NCGroup childGroup) {
                    groups = groups.Append(childGroup);
                    GetNCGroupMembersRecursive(childGroup, ref groups);
                }
        }

        public IEnumerable<NCGroup> GetNCGroupMembers(NCGroup ncGroup, bool recursive = false) {
            IEnumerable<NCGroup> groups = new List<NCGroup>();

            foreach (var member in ncGroup.GetMembers())
                if (member is NCGroup childGroup) {
                    groups = groups.Append(childGroup);

                    if (recursive)
                        GetNCGroupMembersRecursive(childGroup, ref groups);
                }

            return groups;
        }

        public IEnumerable<NCGroup> GetNCGroupMembers(string ncGroupName, bool recursive = false) =>
            GetNCGroupMembers(Find(ncGroupName), recursive);

        public NCGroup Find(string ncGroupName) =>
            Collection.FindObject(ncGroupName);
    }
}
